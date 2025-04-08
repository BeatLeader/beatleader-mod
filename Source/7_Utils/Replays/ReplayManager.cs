using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using JetBrains.Annotations;

namespace BeatLeader.Utils {
    /// <summary>
    /// A class for managing physical replays.
    /// </summary>
    [PublicAPI]
    public static class ReplayManager {
        public const string ReplayFileExtension = ".bsor";

        #region Events

        public static event Action<IReplayHeader>? ReplayAddedEvent;
        public static event Action<IReplayHeader>? ReplayDeletedEvent;
        public static event Action? AllReplaysDeletedEvent;

        public static event Action? LoadingStartedEvent;

        /// <summary>
        /// True if loading has finished. False if was cancelled.
        /// </summary>
        public static event Action<bool>? LoadingFinishedEvent;

        private static void NotifyReplayAdded(IReplayHeader header) {
            ReplayAddedEvent?.Invoke(header);
        }

        #endregion

        #region Replays Loading

        /// <summary>
        /// All loaded replays.
        /// </summary>
        public static IReadOnlyList<IReplayHeader> Headers => headers;

        private static CancellationTokenSource? _loadHeadersCancellationSource;
        private static Task? _loadHeadersTask;
        private static bool _everLoaded;

        /// <summary>
        /// Starts headers loading if never loaded before.
        /// </summary>
        public static void StartLoadingIfNeverLoaded() {
            if (_everLoaded) {
                return;
            }
            
            _everLoaded = true;
            StartLoading();
        }
        
        /// <summary>
        /// Starts headers loading.
        /// </summary>
        public static void StartLoading() {
            _loadHeadersCancellationSource?.Cancel();
            _loadHeadersCancellationSource = new CancellationTokenSource();

            _loadHeadersTask = LoadReplayHeadersAsync(_loadHeadersCancellationSource.Token);

            LoadingStartedEvent?.Invoke();
        }

        /// <summary>
        /// Cancels headers loading.
        /// </summary>
        /// <param name="resetLoadedHeaders">Determines if loaded headers should be reset or not.</param>
        public static void CancelLoading(bool resetLoadedHeaders = true) {
            _loadHeadersCancellationSource?.Cancel();
            _loadHeadersCancellationSource = new CancellationTokenSource();

            _loadHeadersTask = null;

            if (resetLoadedHeaders) {
                headers.Clear();
                hashedHeaders.Clear();
            }

            LoadingFinishedEvent?.Invoke(false);
        }

        /// <summary>
        /// Suspends an execution until headers loading is finished.
        /// </summary>
        public static Task WaitForLoadingAsync() {
            return _loadHeadersTask ?? Task.CompletedTask;
        }

        /// <summary>
        /// Finds a header by the specified info if the header is present in the local storage.
        /// </summary>
        /// <param name="info">An info to calculate the hash from.</param>
        /// <returns>A header for the specified info if present.</returns>
        public static IReplayHeader? FindReplayByHash(IReplayHashProvider info) {
            var hash = info.CalculateReplayHash();

            hashedHeaders.TryGetValue(hash, out var header);

            return header;
        }

        #endregion

        #region Replays Loading Logic

        private static readonly Dictionary<int, IReplayHeader> hashedHeaders = new();
        private static readonly List<IReplayHeader> headers = new();

        // Replay manager does not allow running more than one simultaneous
        // headers task, so we can simply store such things statically
        private static readonly List<Task> tasks = new();

        private static async Task LoadReplayHeadersAsync(CancellationToken token) {
            var paths = FileManager.GetAllReplayPaths().ToArray();

            headers.Clear();
            hashedHeaders.Clear();
            tasks.Clear();

            for (var i = 0; i < paths.Length; i++) {
                var path = paths[i];
                tasks[i] = LoadReplayHeaderAsync(path, token);
            }

            await Task.WhenAll(tasks);

            ReplayHeadersCache.SaveCache();
            LoadingFinishedEvent?.Invoke(true);
        }

        private static async Task LoadReplayHeaderAsync(string path, CancellationToken token) {
            var replayInfo = await LoadReplayInfoAsync(path, token);

            if (replayInfo == null) {
                Plugin.Log.Error($"[ReplayManager] Failed to read replay info: {path}");
                return;
            }

            var hash = replayInfo.CalculateReplayInfoHash();
            if (hashedHeaders.ContainsKey(hash)) {
                Plugin.Log.Debug($"[ReplayManager] Replay info with the same hash already exists. Hash: {hash}");
                return;
            }

            var header = CreateReplayHeader(path, replayInfo);

            headers.Add(header);
            hashedHeaders.Add(hash, header);

            NotifyReplayAdded(header);
        }

        private static async Task<IReplayInfo?> LoadReplayInfoAsync(string path, CancellationToken token) {
            if (ReplayHeadersCache.TryGetInfoByPath(path, out var info)) {
                return info;
            }

            var replayInfo = await FileManager.ReadReplayInfoAsync(path, token);

            if (replayInfo != null) {
                SaturateReplayInfo(replayInfo, path);
                ReplayHeadersCache.AddInfoByPath(path, replayInfo);
                info = replayInfo;
            }

            return info;
        }

        internal static async Task<Replay?> LoadReplayAsync(IReplayHeader header, CancellationToken token) {
            var replay = await FileManager.ReadReplayAsync(header.FilePath, token);

            if (replay != null) {
                SaturateReplayInfo(replay.info, header.FilePath);
                ReplayHeadersCache.AddInfoByPath(header.FilePath, replay.info);
            }

            return replay;
        }

        #endregion

        #region ReplayManager SaveReplay

        public static IReplayHeader? LastSavedReplay { get; private set; }

        /// <summary>
        /// Writes a replay performing configuration checks.
        /// </summary>
        /// <param name="playEndData">Used for name formatting and validation checks, cannot be omitted.</param>
        public static async Task<IReplayHeader?> SaveReplayAsync(Replay replay, PlayEndData playEndData, CancellationToken token) {
            LastSavedReplay = null;

            if (!ShouldSaveReplay(replay, playEndData)) {
                Plugin.Log.Info("[ReplayManager] Validation failed, replay will not be saved!");
                return null;
            }

            if (ConfigFileData.Instance.OverrideOldReplays) {
                Plugin.Log.Warn("[ReplayManager] OverrideOldReplays is enabled, old replays will be deleted");
                await DeleteSimilarReplaysAsync(replay, token);
            }

            SaturateReplay(replay, playEndData);
            return await SaveAnyReplayAsync(replay, playEndData, token);
        }

        /// <summary>
        /// Writes a replay without any validity or config checks.
        /// </summary>
        /// <param name="playEndData">Used for name formatting, not too important.</param>
        public static async Task<IReplayHeader?> SaveAnyReplayAsync(Replay replay, PlayEndData? playEndData, CancellationToken token) {
            var name = FormatFileName(replay, playEndData);
            Plugin.Log.Info($"[ReplayManager] Replay will be saved as: {name}");

            if (!await FileManager.WriteReplayAsync(name, replay, token)) {
                return null;
            }

            var absolutePath = FileManager.GetAbsoluteReplayPath(name);
            var header = CreateReplayHeader(absolutePath, replay.info);

            LastSavedReplay = header;
            headers.Add(header);

            ReplayHeadersCache.AddInfoByPath(header.FilePath, header.ReplayInfo);
            NotifyReplayAdded(header);

            return header;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes all replays.
        /// </summary>
        /// <returns>A count of successfully deleted items.</returns>
        internal static int DeleteAllReplays() {
            ReplayHeadersCache.ClearInfo();
            ReplayHeadersCache.SaveCache();
            ReplayMetadataManager.ClearMetadata();

            var deletedReplays = 0;
            foreach (var replay in Headers) {
                try {
                    File.Delete(replay.FilePath);
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to delete a replay:\n{ex}");
                    continue;
                }
                deletedReplays++;
            }

            headers.Clear();
            AllReplaysDeletedEvent?.Invoke();

            return deletedReplays;
        }

        /// <summary>
        /// Deletes a single replay.
        /// </summary>
        internal static void DeleteReplay(IReplayHeader header) {
            DeleteReplayInternal(header.FilePath, header);
        }

        #endregion

        #region Delete Internal

        private static void FinalizeReplayDeletion(IReplayHeader header) {
            headers.Remove(header);
            (header as BeatLeaderReplayHeader)?.NotifyReplayDeleted();
            ReplayDeletedEvent?.Invoke(header);
        }

        private static void DeleteReplayInternal(string filePath, IReplayHeader? header = null) {
            ReplayHeadersCache.RemoveInfoByPath(filePath);
            ReplayHeadersCache.SaveCache();

            ReplayMetadataManager.DeleteMetadata(filePath);
            File.Delete(filePath);

            if (header != null) {
                FinalizeReplayDeletion(header);
            }
        }

        private static async Task DeleteSimilarReplaysAsync(Replay replay, CancellationToken token) {
            var info = replay.info;
            var buffer = new List<IReplayHeader>();

            await Task.Run(
                () => {
                    foreach (var header in Headers) {
                        if (!CompareReplayInfoForRemoval(header.ReplayInfo, info)) {
                            continue;
                        }

                        Plugin.Log.Info("[ReplayManager] Deleting old replay: " + Path.GetFileName(header.FilePath));

                        DeleteReplayInternal(header.FilePath);
                        buffer.Add(header);
                    }
                },
                token
            );

            foreach (var header in buffer) {
                FinalizeReplayDeletion(header);
            }
        }

        #endregion

        #region Cache

        internal static void LoadCache() {
            ReplayMetadataManager.LoadCache();
        }

        internal static void SaveCache() {
            ReplayMetadataManager.SaveCache();
            ReplayHeadersCache.SaveCache();
        }

        #endregion

        #region Tools

        private static int CalculateReplayInfoHash(this IReplayInfo info) {
            unchecked {
                var hash = 17; // seed
                hash = hash * 31 + info.Timestamp.GetHashCode();
                hash = hash * 31 + info.PlayerID.GetHashCode();

                return hash;
            }
        }

        private static bool CompareReplayInfoForRemoval(IReplayInfo? left, IReplayInfo? right) {
            if (left == null || right == null) {
                return false;
            }

            return left.PlayerID == right.PlayerID
                && left.SongName == right.SongName
                && left.SongDifficulty == right.SongDifficulty
                && left.SongMode == right.SongMode
                && left.SongHash == right.SongHash;
        }

        private static BeatLeaderReplayHeader CreateReplayHeader(string path, IReplayInfo replayInfo) {
            var meta = ReplayMetadataManager.GetMetadata(path);
            return new BeatLeaderReplayHeader(path, replayInfo, meta);
        }

        //TODO: remove after BSOR V2
        private static void SaturateReplay(Replay replay, PlayEndData data) {
            replay.info.levelEndType = data.EndType;
        }

        internal static void SaturateReplayInfo(ReplayInfo info, string? path) {
            if (info.hash.Length > 40 && !info.hash.EndsWith("WIP")) {
                info.hash = info.hash.Substring(0, 40);
            }
            
            if (info.mode is var mode && mode.IndexOf('-') is var idx and not -1) {
                info.mode = mode.Remove(idx, mode.Length - idx);
            }

            if (path != null && Path.GetFileName(path).Contains("exit")) {
                info.levelEndType = LevelEndType.Quit;
            }
        }
        
        private static bool ShouldSaveReplay(Replay replay, PlayEndData endData) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;
            
            return ConfigFileData.Instance.SaveLocalReplays && endData.EndType switch {
                LevelEndType.Fail => options.HasFlag(ReplaySaveOption.Fail),
                LevelEndType.Quit or LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                LevelEndType.Clear => true,
                _ => false
            } && (options.HasFlag(ReplaySaveOption.ZeroScore) || replay.info.score != 0);
        }
        
        private static string FormatFileName(Replay replay, PlayEndData? playEndData) {
            var practice = replay.info.speed != 0 ? "-practice" : "";
            var fail = replay.info.failTime != 0 ? "-fail" : "";
            
            var exit = playEndData?.EndType
                is LevelEndType.Quit
                or LevelEndType.Restart
                ? "-exit" : "";
            
            var info = replay.info;
            var filename = $"{info.playerID}{practice}{fail}{exit}-{info.songName}-{info.difficulty}-{info.mode}-{info.hash}-{info.timestamp}{ReplayFileExtension}";
            
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
            
            return r.Replace(filename, "_");
        }

        #endregion
    }
}
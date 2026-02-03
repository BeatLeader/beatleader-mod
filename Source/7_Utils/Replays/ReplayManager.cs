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
        private const string ReplayFilePattern = "*.bsor";

        #region Events

        public static event Action<IReplayHeader>? ReplayAddedEvent;
        public static event Action<IReplayHeader>? ReplayDeletedEvent;
        public static event Action? AllReplaysDeletedEvent;

        public static event Action? LoadingStartedEvent;

        /// <summary>
        /// True if loading has finished. False if was cancelled.
        /// </summary>
        public static event Action<bool>? LoadingFinishedEvent;

        private static int _lastBatchIndex;

        /// <summary>
        /// Ensures that invocations always happen on the main thread and do not overlap.
        /// </summary>
        private static void SyncNotifyReplaysAdded() {
            SynchronizationContext.Current.Send(
                _ => {
                    var count = headers.Count;

                    for (var i = _lastBatchIndex; i < count; i++) {
                        ReplayAddedEvent?.Invoke(headers[i]);
                    }

                    _lastBatchIndex = count;
                },
                null
            );
        }

        #endregion

        #region Replays Loading

        /// <summary>
        /// All loaded replays.
        /// </summary>
        public static IReadOnlyList<IReplayHeader> Headers => headers;

        /// <summary>
        /// Are headers being loaded or not.
        /// </summary>
        public static bool IsLoading => _loadHeadersTask != null;

        private static CancellationTokenSource _loadHeadersCancellationSource = new();
        private static Task? _loadHeadersTask;
        private static bool _everLoaded;

        /// <summary>
        /// Starts headers loading if never loaded before.
        /// </summary>
        public static bool StartLoadingIfNeverLoaded() {
            if (_everLoaded) {
                return false;
            }

            _everLoaded = true;
            StartLoading();

            return true;
        }

        /// <summary>
        /// Starts headers loading.
        /// </summary>
        public static void StartLoading() {
            if (_loadHeadersTask != null) {
                _loadHeadersCancellationSource.Cancel();
                _loadHeadersCancellationSource = new CancellationTokenSource();
            }

            LoadingStartedEvent?.Invoke();

            _loadHeadersTask = LoadReplayHeadersAsync(_loadHeadersCancellationSource.Token).RunCatching();
        }

        /// <summary>
        /// Cancels headers loading.
        /// </summary>
        /// <param name="resetLoadedHeaders">Determines if loaded headers should be reset or not.</param>
        public static void CancelLoading(bool resetLoadedHeaders = true) {
            _loadHeadersCancellationSource.Cancel();
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
            if (headers.Count != 0) {
                // This event is invoked before the async call so we can safely invoke it without wrappers
                AllReplaysDeletedEvent?.Invoke();
            }

            headers.Clear();
            hashedHeaders.Clear();
            tasks.Clear();
            _lastBatchIndex = 0;

            var paths = FileManager.GetAllReplayPaths();
            foreach (var path in paths) {
                tasks.Add(LoadReplayHeaderAsync(path, token));
            }

            await Task.WhenAll(tasks);

            ReplayHeadersCache.SaveCache();

            // Safely invoke the event on main thread
            await TaskExtensions.RunOnMainThread(() => LoadingFinishedEvent?.Invoke(true));

            _loadHeadersTask = null;
        }

        private static async Task LoadReplayHeaderAsync(string path, CancellationToken token) {
            var replayInfo = await LoadReplayInfoAsync(path, token);

            if (replayInfo == null) {
                Plugin.Log.Error($"[ReplayManager] Failed to read replay info: {path}");
                return;
            }

            var hash = replayInfo.CalculateReplayHash();
            if (hashedHeaders.ContainsKey(hash)) {
                Plugin.Log.Debug($"[ReplayManager] Replay info with the same hash already exists. Hash: {hash}");
                return;
            }

            var header = CreateReplayHeader(path, replayInfo);

            headers.Add(header);
            hashedHeaders.Add(hash, header);

            SyncNotifyReplaysAdded();
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

        public static IReplayHeader? LastPlayedReplay { get; private set; }

        /// <summary>
        /// Writes a replay performing configuration checks.
        /// </summary>
        /// <param name="playEndData">Used for name formatting and validation checks, cannot be omitted.</param>
        public static async Task<ReplaySavingResult> SaveReplayAsync(Replay replay, PlayEndData playEndData, CancellationToken token) {
            LastPlayedReplay = null;

            if (!ShouldSaveReplay(replay, playEndData)) {
                Plugin.Log.Info("[ReplayManager] Validation failed, replay will not be saved!");
                LastPlayedReplay = ReplayManager.CreateTempReplayHeader(replay, null);
                return new(ReplaySavingError.ValidationFailed);
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
        public static async Task<ReplaySavingResult> SaveAnyReplayAsync(Replay replay, PlayEndData? playEndData, CancellationToken token) {
            var hash = replay.info.CalculateReplayHash();

            if (hashedHeaders.TryGetValue(hash, out _)) {
                return new(ReplaySavingError.AlreadyExists);
            }

            var name = FormatFileName(replay, playEndData);
            Plugin.Log.Info($"[ReplayManager] Replay will be saved as: {name}");

            if (!await FileManager.WriteReplayAsync(name, replay, token)) {
                Plugin.Log.Error("[ReplayManager] Failed to write replay");

                return new(ReplaySavingError.WritingFailed);
            }

            var absolutePath = FileManager.GetAbsoluteReplayPath(name);
            var header = CreateReplayHeader(absolutePath, replay.info);

            LastPlayedReplay = header;
            headers.Add(header);
            hashedHeaders.Add(hash, header);

            ReplayHeadersCache.AddInfoByPath(header.FilePath, header.ReplayInfo);
            ReplayAddedEvent?.Invoke(header);

            return new(header);
        }

        /// <summary>
        /// Creates a temporary replay header without adding it to the headers list.
        /// </summary>
        /// <param name="replay">A replay to provide when calling <see cref="IReplayHeader.LoadReplayAsync"/>.</param>
        /// <param name="player">A player to provide when calling <see cref="IReplayHeader.LoadPlayerAsync"/> or null to load it later.</param>
        /// <returns>A temporary header.</returns>
        public static IReplayHeader CreateTempReplayHeader(Replay replay, Player? player) {
            return new TempReplayHeader(replay, player);
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
            foreach (var path in FileManager.GetAllReplayPaths()) {
                try {
                    File.Delete(path);
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to delete a replay:\n{ex}");
                    continue;
                }
                deletedReplays++;
            }

            headers.Clear();
            hashedHeaders.Clear();
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
            (header as PhysicalReplayHeader)?.NotifyReplayDeleted();
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
            ReplayHeadersCache.LoadCache();
        }

        internal static void SaveCache() {
            ReplayMetadataManager.SaveCache();
            ReplayHeadersCache.SaveCache();
        }

        #endregion

        #region Tools

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

        private static PhysicalReplayHeader CreateReplayHeader(string path, IReplayInfo replayInfo) {
            var meta = ReplayMetadataManager.GetMetadata(path);
            return new PhysicalReplayHeader(path, replayInfo, meta);
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

            if (path != null && Path.GetFileName(path).Contains("practice")) {
                info.levelEndType = LevelEndType.Practice;
            }
        }

        private static bool ShouldSaveReplay(Replay replay, PlayEndData endData) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;

            return ConfigFileData.Instance.SaveLocalReplays && endData.EndType switch {
                LevelEndType.Fail                         => options.HasFlag(ReplaySaveOption.Fail),
                LevelEndType.Practice                     => options.HasFlag(ReplaySaveOption.Practice),
                LevelEndType.Quit or LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                LevelEndType.Clear                        => true,
                _                                         => false
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using JetBrains.Annotations;
using static BeatLeader.Utils.FileManager;

namespace BeatLeader.Utils {
    [UsedImplicitly, PublicAPI]
    internal class ReplayManager : Singleton<ReplayManager>, IReplayManager, IReplayFileManager {
        public const string ReplayFileExtension = ".bsor";

        #region ReplayManager Events

        public event Action<IReplayHeader>? ReplayAddedEvent;
        public event Action<IReplayHeader>? ReplayDeletedEvent;
        public event Action? AllReplaysDeletedEvent;

        private void NotifyReplayAdded(IReplayHeader header) {
            ReplayAddedEvent?.Invoke(header);
        }

        private void NotifyReplayDeleted(IReplayHeader header) {
            ReplayDeletedEvent?.Invoke(header);
        }

        #endregion

        #region ReplayManager LoadReplayHeaders

        public IReadOnlyList<IReplayHeader> Replays => _replays;
        public IReplayMetadataManager MetadataManager => ReplayMetadataManager.Instance;

        private readonly HashSet<(string, long)> _headerValuesCache = new();
        private readonly List<IReplayHeader> _temporaryReplays = new();
        private readonly List<IReplayHeader> _replays = new();
        private bool _replaysWereNeverLoaded = true;
        private Task? _loadHeadersTask;

        public Task LoadReplayHeadersAsync(CancellationToken token) {
            if (_loadHeadersTask is null || _loadHeadersTask.IsCompleted) {
                _loadHeadersTask = LoadReplayHeadersAsyncInternal(token);
            }
            return _loadHeadersTask;
        }

        private async Task LoadReplayHeadersAsyncInternal(CancellationToken token) {
            _temporaryReplays.Clear();
            _replaysWereNeverLoaded = false;
            var paths = GetAllReplayPaths();
            var cache = _headerValuesCache;
            await Task.Run(
                () => {
                    foreach (var path in paths) {
                        if (token.IsCancellationRequested) return;
                        var header = LoadReplay(cache, path);
                        if (header is null) continue;
                        _temporaryReplays.Add(header);
                        NotifyReplayAdded(header);
                    }
                },
                token
            );
            _replays.Clear();
            _replays.AddRange(_temporaryReplays);
            cache.Clear();
            ReplayHeadersCache.SaveCache();
        }

        private IReplayHeader? LoadReplay(HashSet<(string, long)> cache, string path) {
            var info = GetReplayInfo(path);
            if (info == null || !cache.Add((info.SongHash, info.Timestamp))) return null;
            return GetReplayHeader(path, info);
        }

        private async Task LoadReplayHeadersIfNeededAsync() {
            if (_replaysWereNeverLoaded) {
                await LoadReplayHeadersAsync(default);
            }
        }

        #endregion

        #region ReplayManager SaveReplay

        public IReplayHeader? CachedReplay { get; private set; }

        public async Task<IReplayHeader?> SaveReplayAsync(Replay replay, PlayEndData playEndData, CancellationToken token) {
            CachedReplay = null;
            if (!ValidatePlay(replay, playEndData)) {
                Plugin.Log.Info("Validation failed, replay will not be saved!");
                return null;
            }
            if (ConfigFileData.Instance.OverrideOldReplays) {
                Plugin.Log.Warn("OverrideOldReplays is enabled, old replays will be deleted");
                await DeleteSimilarReplaysAsync(replay);
            }
            SaturateReplay(replay, playEndData);
            
            var name = FormatFileName(replay, playEndData);
            Plugin.Log.Info($"Replay will be saved as: {name}");
            if (!TryWriteReplay(name, replay)) return null;

            var absolutePath = GetAbsoluteReplayPath(name);
            var header = GetReplayHeader(absolutePath, replay);
            CachedReplay = header;
            _replays.Add(header);
            NotifyReplayAdded(header);

            return header;
        }

        private async Task DeleteSimilarReplaysAsync(Replay replay) {
            await LoadReplayHeadersIfNeededAsync();
            var info = replay.info;
            var buffer = new List<IReplayHeader>();
            //deleting
            foreach (var header in Replays) {
                //
                if (!CompareReplays(header.ReplayInfo, info)) continue;
                Plugin.Log.Info("Deleting old replay: " + Path.GetFileName(header.FilePath));
                //
                DeleteReplayInternal(header.FilePath);
                buffer.Add(header);
            }
            //finalizing deletion
            foreach (var header in buffer) {
                FinalizeReplayDeletion(header);
            }
        }

        private static bool CompareReplays(IReplayInfo? i, IReplayInfo? info) {
            if (i is null || info is null) return false;
            return i.PlayerID == info.PlayerID
                && i.SongName == info.SongName
                && i.SongDifficulty == info.SongDifficulty
                && i.SongMode == info.SongMode
                && i.SongHash == info.SongHash;
        }

        //TODO: remove after BSOR V2
        private static void SaturateReplay(Replay replay, PlayEndData data) {
            replay.info.levelEndType = data.EndType;
        }

        #endregion

        #region ReplayFileManager Save & Delete

        private void FinalizeReplayDeletion(IReplayHeader header) {
            _replays.Remove(header);
            NotifyReplayDeleted(header);
        }
        
        private void DeleteReplayInternal(string filePath, IReplayHeader? header = null) {
            ReplayHeadersCache.RemoveInfoByPath(filePath);
            ReplayHeadersCache.SaveCache();
            ReplayMetadataManager.DeleteMetadata(filePath);
            File.Delete(filePath);
            if (header == null) return;
            FinalizeReplayDeletion(header);
        }

        int IReplayFileManager.DeleteAllReplays() {
            //clearing all cache
            ReplayHeadersCache.ClearInfo();
            ReplayHeadersCache.SaveCache();
            ReplayMetadataManager.ClearMetadata();
            //deleting replays
            var deletedReplays = 0;
            foreach (var replay in Replays) {
                try {
                    File.Delete(replay.FilePath);
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to delete a replay:\n{ex}");
                    continue;
                }
                deletedReplays++;
            }
            _replays.Clear();
            AllReplaysDeletedEvent?.Invoke();
            return deletedReplays;
        }

        bool IReplayFileManager.DeleteReplay(IReplayHeader header) {
            DeleteReplayInternal(header.FilePath, header);
            return true;
        }

        async Task<Replay?> IReplayFileManager.LoadReplayAsync(IReplayHeader header, CancellationToken token) {
            var replay = default(Replay?);
            await Task.Run(() => TryReadReplay(header.FilePath, out replay), token);
            if (replay is null) return replay;
            SaturateReplayInfo(replay.info, header.FilePath);
            ReplayHeadersCache.AddInfoByPath(header.FilePath, replay.info);
            return replay;
        }

        #endregion

        #region Get ReplayHeader & ReplayInfo

        private static GenericReplayHeader GetReplayHeader(string path, Replay replay) {
            var meta = ReplayMetadataManager.GetMetadata(path);
            return new GenericReplayHeader(Instance, path, replay.info, meta);
        }

        private static GenericReplayHeader GetReplayHeader(string path, IReplayInfo replayInfo) {
            var meta = ReplayMetadataManager.GetMetadata(path);
            return new GenericReplayHeader(Instance, path, replayInfo, meta);
        }

        private static IReplayInfo? GetReplayInfo(string path) {
            var cacheLoadSucceed = ReplayHeadersCache.TryGetInfoByPath(path, out var info);
            if (!cacheLoadSucceed && TryReadReplayInfo(path, out var replayInfo)) {
                SaturateReplayInfo(replayInfo!, path);
                ReplayHeadersCache.AddInfoByPath(path, replayInfo!);
                info = replayInfo;
            }
            return info;
        }

        #endregion

        #region Cache

        internal static void LoadCache() {
            ReplayMetadataManager.LoadSerializedCache();
        }
        
        internal static void SaveCache() {
            ReplayMetadataManager.SaveSerializedCache();
            ReplayHeadersCache.SaveCache();
        }

        #endregion

        #region Tools

        internal static void SaturateReplayInfo(ReplayInfo info, string? path) {
            if (info.hash.Length > 40 && !info.hash.EndsWith("WIP")) info.hash = info.hash.Substring(0, 40);
            if (info.mode is var mode && mode.IndexOf('-') is var idx and not -1) {
                info.mode = mode.Remove(idx, mode.Length - idx);
            }
            if (path is not null && Path.GetFileName(path).Contains("exit")) info.levelEndType = LevelEndType.Quit;
        }

        [Pure]
        internal static bool ValidatePlay(Replay replay, PlayEndData endData) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;
            return ConfigFileData.Instance.SaveLocalReplays && endData.EndType switch {
                LevelEndType.Fail => options.HasFlag(ReplaySaveOption.Fail),
                LevelEndType.Quit or LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                LevelEndType.Clear => true,
                _ => false
            } && (options.HasFlag(ReplaySaveOption.ZeroScore) || replay.info.score != 0);
        }

        [Pure]
        internal static string FormatFileName(Replay replay, PlayEndData? playEndData) {
            var practice = replay.info.speed != 0 ? "-practice" : "";
            var fail = replay.info.failTime != 0 ? "-fail" : "";
            var exit = playEndData?.EndType
                is LevelEndType.Quit
                or LevelEndType.Restart
                ? "-exit" : "";
            var info = replay.info;
            var filename = $"{info.playerID}{practice}{fail}{exit}-{info.songName}-{info.difficulty}-{info.mode}-{info.hash}-{info.timestamp}{ReplayFileExtension}";
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, "_");
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Core.Managers.ReplayEnhancer;
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

        private void NotifyReplayAdded(IReplayHeader header) {
            ReplayAddedEvent?.Invoke(header);
        }

        private void NotifyReplayDeleted(IReplayHeader header) {
            ReplayDeletedEvent?.Invoke(header);
        }

        #endregion

        #region ReplayManager LoadReplayHeaders

        public IReadOnlyList<IReplayHeader> Replays => _replays;

        private readonly HashSet<(string, string)> _headerValuesCache = new();
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
                }, token
            );
            _replays.Clear();
            _replays.AddRange(_temporaryReplays);
            cache.Clear();
            ReplayHeadersCache.SaveCache();
        }

        private IReplayHeader? LoadReplay(HashSet<(string, string)> cache, string path) {
            var cacheLoadSucceed = ReplayHeadersCache.TryGetInfoByPath(path, out var info);
            if (!cacheLoadSucceed) {
                if (TryReadReplayInfo(path, out var replayInfo)) SaturateReplayInfo(replayInfo!, path);
                info = replayInfo;
            }

            if (info is null || !cache.Add((info.SongHash, info.Timestamp))) return null;
            if (!cacheLoadSucceed) ReplayHeadersCache.AddInfoByPath(path, info);

            return new GenericReplayHeader(this, path, info);
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
            var isOstLevel = !MapEnhancer.previewBeatmapLevel
                .levelID.StartsWith(CustomLevelLoader.kCustomLevelPrefixId);
            CachedReplay = null;
            if (!ValidatePlay(replay, playEndData, isOstLevel)) {
                Plugin.Log.Info("Validation failed, replay will not be saved!");
                return null;
            }

            if (ConfigFileData.Instance.OverrideOldReplays) {
                Plugin.Log.Warn("OverrideOldReplays is enabled, old replays will be deleted");
                await DeleteSimilarReplaysAsync(replay);
            }

            SaturateReplay(replay, playEndData);
            var path = FormatFileName(replay, playEndData);
            Plugin.Log.Info($"Replay will be saved as: {path}");
            if (!TryWriteReplay(path, replay)) return null;

            var absolutePath = GetAbsoluteReplayPath(path);
            var header = new GenericReplayHeader(this, absolutePath, replay);
            CachedReplay = header;
            _replays.Add(header);
            NotifyReplayAdded(header);

            return header;
        }

        private async Task DeleteSimilarReplaysAsync(Replay replay) {
            await LoadReplayHeadersIfNeededAsync();
            var info = replay.info;
            foreach (var h in Replays) {
                if (!CompareReplays(h.ReplayInfo, info)) continue;
                Plugin.Log.Info("Deleting old replay: " + Path.GetFileName(h.FilePath));
                h.DeleteReplay();
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

        private static bool ValidatePlay(Replay replay, PlayEndData endData, bool isOstLevel) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;
            return ConfigFileData.Instance.SaveLocalReplays && endData.EndType switch {
                    LevelEndType.Fail => options.HasFlag(ReplaySaveOption.Fail),
                    LevelEndType.Quit or LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                    LevelEndType.Clear => true,
                    _ => false
                } && (options.HasFlag(ReplaySaveOption.ZeroScore) || replay.info.score != 0)
                && (options.HasFlag(ReplaySaveOption.OST) || !isOstLevel);
        }

        #endregion

        #region ReplayFileManager Save & Delete

        private void DeleteReplayInternal(string filePath, IReplayHeader? header = null) {
            ReplayHeadersCache.RemoveInfoByPath(filePath);
            ReplayHeadersCache.SaveCache();
            File.Delete(filePath);
            if (header is null) return;
            _replays.Remove(header);
            NotifyReplayDeleted(header);
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

        #region Tools

        internal static void SaturateReplayInfo(ReplayInfo info, string? path) {
            if (info.hash.Length > 40 && !info.hash.EndsWith("WIP")) info.hash = info.hash.Substring(0, 40);
            if (info.mode is var mode && mode.IndexOf('-') is var idx and not -1) {
                info.mode = mode.Remove(idx, mode.Length - idx);
            }
            if (path is not null && Path.GetFileName(path).Contains("exit")) info.levelEndType = LevelEndType.Quit;
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
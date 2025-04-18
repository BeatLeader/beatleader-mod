﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class ReplayManager : Singleton<ReplayManager>, IReplayManager, IReplayFileManager {
        private const int PreloadedReplaysCount = 1024;
        public const string ReplayFileExtension = ".bsor";

        #region Impl

        public event Action<IReplayHeader>? ReplayAddedEvent;
        public event Action<IReplayHeader>? ReplayDeletedEvent;
        public event Action<string[]?>? ReplaysDeletedEvent;

        public IReplayHeader? CachedReplay { get; private set; }

        private IList<IReplayHeader>? _lastReplayHeaders;

        public async Task<IList<IReplayHeader>?> LoadReplayHeadersAsync(
            CancellationToken token,
            Action<IReplayHeader>? loadCallback = null
        ) {
            var paths = GetAllReplayPaths();
            var replays = new List<IReplayHeader>(PreloadedReplaysCount);
            var cache = new HashSet<(string, long)>(PreloadedReplaysCount);
            await Task.Run(() => {
                foreach (var path in paths) {
                    if (token.IsCancellationRequested) return;
                    if (ReplayHeadersCache.TryGetInfoByPath(path, out var info) is var fromCache && !fromCache) {
                        if (TryReadReplayInfo(path, out var replayInfo)) ValidateReplayInfo(replayInfo!, path);
                        info = replayInfo;
                    }

                    if (info is null || !cache.Add((info.SongHash, info.Timestamp))) continue;
                    if (!fromCache) ReplayHeadersCache.AddInfoByPath(path, info);

                    var header = new GenericReplayHeader(this, path, info);
                    replays.Add(header);
                    loadCallback?.Invoke(header);
                }
            }, token);
            cache.Clear();
            _lastReplayHeaders = replays;
            ReplayHeadersCache.SaveCache();
            return replays;
        }

        public async Task<IReplayHeader?> SaveReplayAsync(Replay replay, PlayEndData playEndData, CancellationToken token) {
            CachedReplay = null;
            if (!ValidatePlay(replay, playEndData)) {
                Plugin.Log.Info("Validation failed, replay will not be saved!");
                var tempheader = new GenericReplayHeader(this, "", replay);
                CachedReplay = tempheader;
                return null;
            }

            var header = await Task.Run(async () => {
                var path = FormatFileName(replay, playEndData);
                Plugin.Log.Info($"Replay will be saved as: {path}");
                if (ConfigFileData.Instance.OverrideOldReplays) {
                    Plugin.Log.Warn("OverrideOldReplays is enabled, old replays will be deleted");
                    var info = replay.info;
                    if (_lastReplayHeaders is null) await LoadReplayHeadersAsync(token);
                    foreach (var replayHeader in _lastReplayHeaders!.Where(x =>
                            x.ReplayInfo is { } i && i.PlayerID == info.playerID
                            && i.SongName == info.songName && i.SongDifficulty == info.difficulty
                            && i.SongMode == info.mode && i.SongHash == info.hash).ToArray()
                    ) {
                        Plugin.Log.Info("Deleting old replay: " + Path.GetFileName(replayHeader.FilePath));
                        ((IReplayFileManager)this).DeleteReplay(replayHeader);
                    }
                }

                if (!TryWriteReplay(path, replay)) return null;
                replay.info.levelEndType = playEndData.EndType;
                var absolutePath = GetAbsoluteReplayPath(path);
                return new GenericReplayHeader(this, absolutePath, replay);
            });
            if (header == null) {
                return null;
            }

            _lastReplayHeaders?.Add(header);
            CachedReplay = header;
            ReplayAddedEvent?.Invoke(header);
            return header;
        }

        public async Task<string[]?> DeleteAllReplaysAsync(CancellationToken token) {
            var removedPaths = new List<string>();
            await Task.Run(() => {
                foreach (var path in GetAllReplayPaths()) {
                    if (token.IsCancellationRequested) return;
                    try {
                        DeleteReplayInternal(path);
                        removedPaths.Add(path);
                    } catch (Exception ex) {
                        Plugin.Log.Error("Failed to delete the replay! \n" + ex);
                    }
                }
            }, token);
            //sadly CollectionsMarshal was added only in dotnet 5, so we forced to create a new array
            var removedPathsArr = removedPaths.ToArray();
            ReplaysDeletedEvent?.Invoke(removedPathsArr);
            return removedPathsArr;
        }

        bool IReplayFileManager.DeleteReplay(IReplayHeader header) {
            DeleteReplayInternal(header.FilePath, header);
            return true;
        }

        private void DeleteReplayInternal(string filePath, IReplayHeader? header = null) {
            ReplayHeadersCache.RemoveInfoByPath(filePath);
            ReplayHeadersCache.SaveCache();
            File.Delete(filePath);
            if (header is null) return;
            _lastReplayHeaders?.Remove(header);
            ReplayDeletedEvent?.Invoke(header);
        }

        async Task<Replay?> IReplayFileManager.LoadReplayAsync(IReplayHeader header, CancellationToken token) {
            var replay = default(Replay?);
            await Task.Run(() => TryReadReplay(header.FilePath, out replay), token);
            if (replay is null) return replay;
            ValidateReplayInfo(replay.info, header.FilePath);
            ReplayHeadersCache.AddInfoByPath(header.FilePath, replay.info);
            return replay;
        }

        #endregion

        private static bool IsSHA1Hash(string hash) {
            return new Regex(@"\b[0-9a-fA-F]{40}\b").IsMatch(hash);
        }
        
        internal static void ValidateReplayInfo(ReplayInfo info, string? path) {
            //truncating hash if needed
            var customLevel = IsSHA1Hash(info.hash);
            var wipLevel = info.hash.EndsWith("WIP");
            if (customLevel && !wipLevel && info.hash.Length > 40) {
                info.hash = info.hash.Substring(0, 40);
            }
            //
            if (info.mode is var mode && mode.IndexOf('-') is var idx and not -1) {
                info.mode = mode.Remove(idx, mode.Length - idx);
            }
            if (path is not null && Path.GetFileName(path).Contains("exit")) {
                info.levelEndType = LevelEndType.Quit;
            }
        }

        [Pure]
        internal static bool ValidatePlay(Replay replay, PlayEndData endData) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;
            return ConfigFileData.Instance.SaveLocalReplays && endData.EndType switch {
                    LevelEndType.Fail => options.HasFlag(ReplaySaveOption.Fail),
                    LevelEndType.Quit or LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                    LevelEndType.Practice => options.HasFlag(ReplaySaveOption.Practice),
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
    }
}
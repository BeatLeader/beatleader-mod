using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Activity;
using BeatLeader.Models.Replay;
using JetBrains.Annotations;
using static BeatLeader.Utils.FileManager;

namespace BeatLeader.Utils {
    [PublicAPI]
    public class ReplayManagerStorage : Singleton<IReplayManager> {
        static ReplayManagerStorage() {
            AssignFactory(() => ReplayManager.Instance);
        }
    }
    
    [UsedImplicitly]
    internal class ReplayManager : Singleton<ReplayManager>, IReplayManager {
        private const int PreloadedReplaysCount = 1000;

        public event Action<IReplayHeader>? ReplayAddedEvent;
        public event Action<IReplayHeader>? ReplayDeletedEvent;
        public event Action<string[]?>? ReplaysDeletedEvent;

        public IReplayHeader? LastSavedReplay { get; private set; }

        private IEnumerable<IReplayHeader>? _lastReplayHeaders;

        public async Task<IEnumerable<IReplayHeader>?> LoadReplayHeadersAsync(
            CancellationToken token,
            Action<IReplayHeader>? loadCallback = null,
            bool makeArray = true
        ) {
            var paths = GetAllReplayPaths();
            var replays = makeArray ? new List<IReplayHeader>(PreloadedReplaysCount) : null;
            var cache = new HashSet<(string, string)>();
            await Task.Run(() => {
                foreach (var path in paths) {
                    if (token.IsCancellationRequested) return;
                    TryReadReplayInfo(path, out var info);
                    if (info is not null && !cache.Add((info.hash, info.timestamp))) continue;
                    var header = new GenericReplayHeader(this, path, info);
                    if (makeArray) replays!.Add(header);
                    loadCallback?.Invoke(header);
                }
            }, token);
            cache.Clear();
            _lastReplayHeaders = replays;
            return replays;
        }

        public Task<IReplayHeader?> SaveReplayAsync(Replay replay, PlayEndData playEndData, CancellationToken token) {
            var path = ToFileName(replay, playEndData);
            Plugin.Log.Debug($"Replay will be saved as: {path}");
            if (ConfigFileData.Instance.OverrideOldReplays
                && _lastReplayHeaders is not null) {
                Plugin.Log.Warn("OverrideOldReplays is enabled, old replays will be deleted");
                var info = replay.info;
                foreach (var replayHeader in _lastReplayHeaders) {
                    if (replayHeader.ReplayInfo is not { } replayInfo ||
                        replayInfo.playerID != info.playerID ||
                        replayInfo.songName != info.songName ||
                        replayInfo.difficulty != info.difficulty
                        || replayInfo.mode != info.mode
                        || replayInfo.hash != info.hash) continue;
                    Plugin.Log.Info("Deleting old replay: " + Path.GetFileName(replayHeader.FilePath));
                    ((IReplayManager)this).DeleteReplayAsync(replayHeader, default);
                }
            }
            if (!TryWriteReplay(path, replay)) return Task.FromResult<IReplayHeader?>(null);
            var header = new GenericReplayHeader(this, path, replay);
            ReplayAddedEvent?.Invoke(header);
            LastSavedReplay = header;
            return Task.FromResult<IReplayHeader?>(header);
        }

        internal void ResetLastReplay() {
            LastSavedReplay = null;
        }

        [Pure]
        internal bool ValidatePlay(Replay replay, PlayEndData endData, bool isOstLevel) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;
            return ConfigFileData.Instance.SaveLocalReplays && endData.EndType switch {
                    PlayEndData.LevelEndType.Fail => options.HasFlag(ReplaySaveOption.Fail),
                    PlayEndData.LevelEndType.Quit or PlayEndData.LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                    PlayEndData.LevelEndType.Clear => true,
                    _ => false
                } && (options.HasFlag(ReplaySaveOption.ZeroScore) || replay.info.score != 0)
                && (options.HasFlag(ReplaySaveOption.OST) || !isOstLevel);
        }

        Task<bool> IReplayManager.DeleteReplayAsync(IReplayHeader header, CancellationToken token) {
            File.Delete(header.FilePath);
            ReplayDeletedEvent?.Invoke(header);
            return Task.FromResult(true);
        }

        public async Task<string[]?> DeleteAllReplaysAsync(CancellationToken token) {
            var removedPaths = new List<string>();
            await Task.Run(() => {
                foreach (var path in GetAllReplayPaths()) {
                    if (token.IsCancellationRequested) return;
                    try {
                        File.Delete(path);
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

        async Task<Replay?> IReplayManager.LoadReplayAsync(IReplayHeader header, CancellationToken token) {
            var replay = default(Replay?);
            await Task.Run(() => TryReadReplay(header.FilePath, out replay), token);
            return replay;
        }
    }
}
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
    public sealed class ReplayManager : Singleton<ReplayManager>, IReplayManager {
        private const int PreloadedReplaysCount = 200;

        public event Action<IReplayHeader>? ReplayAddedEvent;
        public event Action<IReplayHeader>? ReplayDeletedEvent;
        public event Action<string[]?>? ReplaysDeletedEvent;

        public IReplayHeader? LastSavedReplay { get; private set; }

        public bool splitTasks;

        public async Task<IEnumerable<IReplayHeader>> LoadReplayHeadersAsync(CancellationToken token) {
            var paths = GetAllReplayPaths();
            var replays = new List<IReplayHeader>(PreloadedReplaysCount);
            var tasks = splitTasks ? new List<Task>(PreloadedReplaysCount) : null;
            if (splitTasks) {
                DecodeAndAddReplays();
                await Task.WhenAll(tasks!);
            } else {
                await Task.Run(DecodeAndAddReplays, token);
            }
            return replays;

            void DecodeAndAddReplays() {
                foreach (var path in paths) {
                    if (!splitTasks) {
                        DecodeAndAdd(this, replays, path);
                        continue;
                    }
                    var task = Task.Run(() => DecodeAndAdd(this, replays, path), token);
                    tasks!.Add(task);
                }

                static void DecodeAndAdd(IReplayManager replayManager, ICollection<IReplayHeader> replays, string path) {
                    TryReadReplayInfo(path, out var info);
                    replays.Add(new GenericReplayHeader(replayManager, path, info));
                }
            }
        }

        public Task<IReplayHeader?> SaveReplayAsync(Replay replay, CancellationToken token) {
            var path = ToFileName(replay);
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
        internal bool ValidatePlay(Replay replay, PlayEndData endData) {
            var options = ConfigFileData.Instance.ReplaySavingOptions;
            return endData.EndType switch {
                PlayEndData.LevelEndType.Fail => options.HasFlag(ReplaySaveOption.Fail),
                PlayEndData.LevelEndType.Quit or PlayEndData.LevelEndType.Restart => options.HasFlag(ReplaySaveOption.Exit),
                PlayEndData.LevelEndType.Clear => true,
                _ => false
            } && (options.HasFlag(ReplaySaveOption.ZeroScore) || replay.info.score != 0);
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
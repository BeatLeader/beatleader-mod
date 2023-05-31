using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using JetBrains.Annotations;
using static BeatLeader.Utils.FileManager;

namespace BeatLeader.Utils {
    [PublicAPI]
    public sealed class ReplayManager : IReplayManager {
        private const int PreloadedReplaysCount = 200;
        
        public static readonly IReplayManager Instance = new ReplayManager();
        
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

        public Task<bool> DeleteReplayAsync(IReplayHeader header, CancellationToken token) {
            File.Delete(header.FilePath);
            return Task.FromResult(true);
        }

        public async Task<Replay?> LoadReplayAsync(IReplayHeader header, CancellationToken token) {
            var replay = default(Replay?);
            await Task.Run(() => TryReadReplay(header.FilePath, out replay), token);
            return replay;
        }
    }
}
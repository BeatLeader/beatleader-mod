using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Models {
    internal class TempReplayHeader : ReplayHeaderBase, IReplayHeader {
        public TempReplayHeader(Replay.Replay replay, Player? player) : base(player) {
            _replay = replay;
        }

        public override IReplayInfo ReplayInfo => _replay.info;
        public ReplayMetadata ReplayMetadata { get; } = new(); 
        public FileStatus FileStatus => FileStatus.Loaded;
        public string FilePath => "Temporary";

        public event Action<FileStatus>? StatusChangedEvent;

        private readonly Replay.Replay _replay;

        public Task<Replay.Replay?> LoadReplayAsync(CancellationToken token) {
            return Task.FromResult(_replay)!;
        }
    }
}
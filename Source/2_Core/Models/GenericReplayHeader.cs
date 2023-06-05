using System;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Replay;

namespace BeatLeader.Models {
    public class GenericReplayHeader : IReplayHeader {
        public GenericReplayHeader(
            IReplayManager replayManager,
            string filePath,
            ReplayInfo? replayInfo
        ) {
            _replayManager = replayManager;
            FilePath = filePath;
            Info = replayInfo;
            _status = replayInfo is null ? ReplayStatus.Corrupted : ReplayStatus.Unloaded;
        }

        public GenericReplayHeader(
            IReplayManager replayManager,
            string filePath,
            Replay.Replay replay
        ) : this(replayManager, filePath, replay.info) {
            _cachedReplay = replay;
            _status = ReplayStatus.Loaded;
        }

        private readonly IReplayManager _replayManager;
        private Replay.Replay? _cachedReplay;

        public string FilePath { get; }
        public ReplayInfo? Info { get; private set; }
        public ReplayStatus Status {
            get => _status;
            private set {
                _status = value;
                StatusChangedEvent?.Invoke(value);
            }
        }

        public event Action<ReplayStatus>? StatusChangedEvent;

        private ReplayStatus _status;

        public async Task<Replay.Replay?> LoadReplayAsync(CancellationToken token) {
            if (_cachedReplay is not null) return _cachedReplay;
            Status = ReplayStatus.Loading;
            _cachedReplay = await _replayManager.LoadReplayAsync(this, token);
            Status = _cachedReplay is null ? ReplayStatus.Corrupted : ReplayStatus.Loaded;
            return _cachedReplay;
        }

        public async Task<bool> DeleteReplayAsync(CancellationToken token) {
            if (Status is ReplayStatus.Deleted || !await _replayManager
                .DeleteReplayAsync(this, token)) return false;
            Info = null;
            Status = ReplayStatus.Deleted;
            return true;
        }
    }
}
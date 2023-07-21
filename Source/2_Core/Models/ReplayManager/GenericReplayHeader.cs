using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Models {
    public class GenericReplayHeader : IReplayHeader {
        public GenericReplayHeader(
            IReplayFileManager replayManager,
            string filePath,
            IReplayInfo? replayInfo
        ) {
            _replayManager = replayManager;
            FilePath = filePath;
            ReplayInfo = replayInfo;
            _status = replayInfo is null ? FileStatus.Corrupted : FileStatus.Unloaded;
        }

        public GenericReplayHeader(
            IReplayFileManager replayManager,
            string filePath,
            Replay.Replay replay
        ) : this(replayManager, filePath, replay.info) {
            _cachedReplay = replay;
            _status = FileStatus.Loaded;
        }

        private readonly IReplayFileManager _replayManager;
        private Replay.Replay? _cachedReplay;

        public FileStatus FileStatus {
            get => _status;
            private set {
                _status = value;
                StatusChangedEvent?.Invoke(value);
            }
        }
        public string FilePath { get; }
        public IReplayInfo? ReplayInfo { get; private set; }

        public event Action<FileStatus>? StatusChangedEvent;

        private FileStatus _status;

        public async Task<Replay.Replay?> LoadReplayAsync(CancellationToken token) {
            if (_cachedReplay is not null) return _cachedReplay;
            FileStatus = FileStatus.Loading;
            _cachedReplay = await _replayManager.LoadReplayAsync(this, token);
            FileStatus = _cachedReplay is null ? FileStatus.Corrupted : FileStatus.Loaded;
            return _cachedReplay;
        }

        public async Task<bool> DeleteReplayAsync(CancellationToken token) {
            if (FileStatus is FileStatus.Deleted || !await _replayManager
                .DeleteReplayAsync(this, token)) return false;
            ReplayInfo = null;
            FileStatus = FileStatus.Deleted;
            return true;
        }
    }
}
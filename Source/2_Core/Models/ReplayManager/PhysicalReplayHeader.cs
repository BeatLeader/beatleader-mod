using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Utils;

namespace BeatLeader.Models {
    internal class PhysicalReplayHeader : ReplayHeaderBase, IReplayHeader {
        public PhysicalReplayHeader(
            string filePath,
            IReplayInfo replayInfo,
            ReplayMetadata metadata
        ) : base(null) {
            FilePath = filePath;
            ReplayInfo = replayInfo;
            ReplayMetadata = metadata;
            _status = FileStatus.Unloaded;
        }

        public FileStatus FileStatus {
            get => _status;
            private set {
                _status = value;
                StatusChangedEvent?.Invoke(value);
            }
        }

        public string FilePath { get; }
        public override IReplayInfo ReplayInfo { get; }
        public ReplayMetadata ReplayMetadata { get; }

        public event Action<FileStatus>? StatusChangedEvent;

        private readonly SemaphoreSlim _loadReplaySemaphore = new(1, 1);
        
        private FileStatus _status;
        private Replay.Replay? _cachedReplay;

        public async Task<Replay.Replay?> LoadReplayAsync(CancellationToken token) {
            await _loadReplaySemaphore.WaitAsync(token);

            if (_cachedReplay != null) {
                _loadReplaySemaphore.Release();
                return _cachedReplay;
            }

            FileStatus = FileStatus.Loading;
            _cachedReplay = await ReplayManager.LoadReplayAsync(this, token);
            FileStatus = _cachedReplay == null ? FileStatus.Corrupted : FileStatus.Loaded;

            _loadReplaySemaphore.Release();

            return _cachedReplay;
        }

        public void NotifyReplayDeleted() {
            FileStatus = FileStatus.Deleted;
        }
    }
}
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
        private WeakReference<WeakRefWrapper<Replay.Replay>>? _cachedReplay;

        public async Task<Replay.Replay?> LoadReplayAsync(CancellationToken token) {
            await _loadReplaySemaphore.WaitAsync(token);

            if (_status is FileStatus.Corrupted) {
                return null;
            }

            if (_cachedReplay?.TryGetTarget(out var replayHolder) ?? false) {
                _loadReplaySemaphore.Release();
                return replayHolder.Object;
            }
            
            FileStatus = FileStatus.Loading;
            var replay = await ReplayManager.LoadReplayAsync(this, token);
            
            FileStatus = replay == null ? FileStatus.Corrupted : FileStatus.Loaded;

            if (replay != null) {
                var wrapper = new WeakRefWrapper<Replay.Replay>(replay);
                wrapper.ObjectDestroyedEvent += HandleReplayUnloaded;

                if (_cachedReplay == null) {
                    _cachedReplay = new(wrapper);
                } else {
                    _cachedReplay.SetTarget(wrapper);
                }
            }

            _loadReplaySemaphore.Release();

            return replay;
        }

        public void NotifyReplayDeleted() {
            FileStatus = FileStatus.Deleted;
        }

        private void HandleReplayUnloaded() {
            FileStatus = FileStatus.Unloaded;
        }
    }
}
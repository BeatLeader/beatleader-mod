using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Activity;
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
            ReplayInfo = replayInfo;
            _status = replayInfo is null ? FileStatus.Corrupted : FileStatus.Unloaded;
            var filename = Path.GetFileNameWithoutExtension(filePath);
            ReplayFinishType = 
                filename.Contains("exit") ? PlayEndData.LevelEndType.Quit :
                filename.Contains("fail") ? PlayEndData.LevelEndType.Fail : 
                PlayEndData.LevelEndType.Clear;
        }

        public GenericReplayHeader(
            IReplayManager replayManager,
            string filePath,
            Replay.Replay replay
        ) : this(replayManager, filePath, replay.info) {
            _cachedReplay = replay;
            _status = FileStatus.Loaded;
        }

        private readonly IReplayManager _replayManager;
        private Replay.Replay? _cachedReplay;

        public string FilePath { get; }
        public FileStatus FileStatus {
            get => _status;
            private set {
                _status = value;
                StatusChangedEvent?.Invoke(value);
            }
        }
        
        public ReplayInfo? ReplayInfo { get; private set; }
        public PlayEndData.LevelEndType ReplayFinishType { get; private set; }

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
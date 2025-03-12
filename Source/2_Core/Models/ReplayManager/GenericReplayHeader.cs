using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Utils;

namespace BeatLeader.Models {
    public class GenericReplayHeader : IReplayHeader {
        public GenericReplayHeader(
            IReplayFileManager replayManager,
            string filePath,
            IReplayInfo replayInfo,
            IReplayMetadata metadata
        ) {
            _replayManager = replayManager;
            FilePath = filePath;
            ReplayInfo = replayInfo;
            ReplayMetadata = metadata;
            _status = FileStatus.Unloaded;
        }

        public GenericReplayHeader(
            IReplayFileManager replayManager,
            string filePath,
            Replay.Replay replay,
            IReplayMetadata metadata
        ) : this(replayManager, filePath, replay.info, metadata) {
            _cachedReplay = replay;
            _status = FileStatus.Loaded;
        }

        public FileStatus FileStatus {
            get => _status;
            private set {
                _status = value;
                StatusChangedEvent?.Invoke(value);
            }
        }

        public string FilePath { get; }
        public IReplayInfo ReplayInfo { get; private set; }
        public IReplayMetadata ReplayMetadata { get; }

        public event Action<FileStatus>? StatusChangedEvent;

        private readonly SemaphoreSlim _loadReplaySemaphore = new(1, 1);
        private readonly SemaphoreSlim _loadPlayerSemaphore = new(1, 1);

        private readonly IReplayFileManager _replayManager;
        private FileStatus _status;
        private Replay.Replay? _cachedReplay;
        private IPlayer? _cachedPlayer;

        public async Task<Replay.Replay?> LoadReplayAsync(CancellationToken token) {
            await _loadReplaySemaphore.WaitAsync(token);

            if (_cachedReplay != null) {
                _loadReplaySemaphore.Release();
                return _cachedReplay;
            }

            FileStatus = FileStatus.Loading;
            _cachedReplay = await _replayManager.LoadReplayAsync(this, token);
            FileStatus = _cachedReplay == null ? FileStatus.Corrupted : FileStatus.Loaded;

            _loadReplaySemaphore.Release();

            return _cachedReplay;
        }

        public async Task<IPlayer> LoadPlayerAsync(bool bypassCache, CancellationToken token) {
            await _loadPlayerSemaphore.WaitAsync(token);

            if (!bypassCache) {
                _cachedPlayer ??= ReplayManagerCache.GetPlayer(ReplayInfo.PlayerID);

                if (_cachedPlayer != null) {
                    _loadPlayerSemaphore.Release();
                    return _cachedPlayer;
                }
            }

            var request = PlayerRequest.SendRequest(ReplayInfo.PlayerID);
            await request.Join();

            if (request.RequestStatusCode is not HttpStatusCode.OK) {
                Plugin.Log.Error($"Failed to load player(id: {ReplayInfo.PlayerID}) from the server!");
            } else {
                _cachedPlayer = request.Result;
                ReplayManagerCache.AddPlayer(_cachedPlayer!);
            }

            _loadPlayerSemaphore.Release();

            return _cachedPlayer ?? Player.GuestPlayer;
        }

        public bool DeleteReplay() {
            if (FileStatus is FileStatus.Deleted || !_replayManager.DeleteReplay(this)) return false;
            ReplayInfo = null;
            FileStatus = FileStatus.Deleted;
            return true;
        }
    }
}
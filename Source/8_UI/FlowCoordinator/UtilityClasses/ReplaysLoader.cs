using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class ReplaysLoader : IReplaysLoader {
        [Inject] private readonly IReplayManager _replayManager = null!;

        public IReadOnlyList<IReplayHeader> LoadedReplays => _replayManager.Replays;

        public event Action<IReplayHeader>? ReplayLoadedEvent {
            add => _replayManager.ReplayAddedEvent += value;
            remove => _replayManager.ReplayAddedEvent -= value;
        }

        public event Action<IReplayHeader>? ReplayRemovedEvent {
            add => _replayManager.ReplayDeletedEvent += value;
            remove => _replayManager.ReplayDeletedEvent -= value;
        }

        public event Action? AllReplaysRemovedEvent;

        public event Action? ReplaysLoadStartedEvent;
        public event Action? ReplaysLoadFinishedEvent;

        private CancellationTokenSource _cancellationTokenSource = new();
        private Task? _loadHeadersTask;

        public void StartReplaysLoad() {
            if (_loadHeadersTask is not null) return;
            LoadHeadersAsync().ConfigureAwait(true);
        }

        public void CancelReplaysLoad() {
            if (_loadHeadersTask is null) return;
            _cancellationTokenSource.Cancel();
        }

        public Task WaitForReplaysLoad() {
            return _loadHeadersTask ?? Task.CompletedTask;
        }

        private async Task LoadHeadersAsync() {
            _cancellationTokenSource = new();
            AllReplaysRemovedEvent?.Invoke();
            _loadHeadersTask = _replayManager.LoadReplayHeadersAsync(_cancellationTokenSource.Token);
            ReplaysLoadStartedEvent?.Invoke();
            await _loadHeadersTask;
            _loadHeadersTask = null;
            ReplaysLoadFinishedEvent?.Invoke();
        }
    }
}
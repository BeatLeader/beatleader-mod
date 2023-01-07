using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class ReplayFinishTweak : GameTweak {
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;

        private bool _wasFinished;

        public override void Initialize() {
            _timeController.SongReachedReplayEndEvent += HandleReplayFinished;
            _timeController.EarlySongWasRewoundEvent += HandleSongRewound;
        }

        public override void Dispose() {
            _timeController.SongReachedReplayEndEvent -= HandleReplayFinished;
            _timeController.EarlySongWasRewoundEvent -= HandleSongRewound;
        }

        private void HandleReplayFinished() {
            if (_finishController.ExitAutomatically) return;
            _pauseController.Pause();
            _pauseController.LockUnpause = true;
            _wasFinished = true;
        }

        private void HandleSongRewound(float time) {
            if (_wasFinished) {
                _pauseController.LockUnpause = false;
                _wasFinished = false;
            }
        }
    }
}

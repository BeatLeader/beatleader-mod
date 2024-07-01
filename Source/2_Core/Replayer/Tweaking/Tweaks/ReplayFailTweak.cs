using BeatLeader.Models;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class ReplayFailTweak : GameTweak {
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly GameEnergyCounter _energyCounter = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override void Initialize() {
            _timeController.SongReachedReplayEndEvent += HandleReplayFinished;
        }

        public override void Dispose() {
            _timeController.SongReachedReplayEndEvent -= HandleReplayFinished;
        }

        private void HandleReplayFinished() {
            //TODO: add br support
            if (_launchData.MainReplay.ReplayData.FinishTime <= _timeController.SongTime) return;
            _energyCounter.ProcessEnergyChange(-1f);
        }
    }
}
using BeatLeader.Models;
using UnityEngine;
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
            if (Mathf.Abs(_launchData.MainReplay.ReplayData.FailTime - _timeController.SongTime) > 0.05f) return;
            _energyCounter.ProcessEnergyChange(-1f);
        }
    }
}
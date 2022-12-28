using BeatLeader.Models;
using System;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayTimeController : BeatmapTimeController, IReplayTimeController {
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public float ReplayEndTime {
            get {
                var failTime = _launchData.MainReplay.info.failTime;
                var time = failTime == 0 ? SongEndTime : failTime;
                return _launchData.IsBattleRoyale ? SongEndTime : time;
            }
        }

        public event Action? SongReachedReplayEndEvent;

        private bool _songReachedReplayEnd;

        public override void Rewind(float time, bool resumeAfterRewind = true) {
            time = Mathf.Clamp(time, SongStartTime, ReplayEndTime);
            base.Rewind(time, resumeAfterRewind);
            _songReachedReplayEnd = false;
        }

        private void Update() {
            if (!_songReachedReplayEnd && Mathf.Abs(SongTime - ReplayEndTime) < 0.1f) {
                SongReachedReplayEndEvent?.Invoke();
                _songReachedReplayEnd = true;
            }
        }
    }
}

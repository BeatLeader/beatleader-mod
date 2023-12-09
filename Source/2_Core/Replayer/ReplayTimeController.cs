using BeatLeader.Models;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayTimeController : BeatmapTimeController, IReplayTimeController {
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public float ReplayEndTime {
            get {
                if (_replayEndTime is -1) {
                    var replays = _launchData.Replays;
                    var maxReplayFinishTime = replays.Select(x => x.ReplayData.FinishTime).Max();
                    _replayEndTime = maxReplayFinishTime;
                }
                return _replayEndTime;
            }
        }

        public event Action? SongReachedReplayEndEvent;

        private bool _songReachedReplayEnd;
        private float _replayEndTime = -1;

        public override void Rewind(float time, bool resumeAfterRewind = true) {
            time = Mathf.Clamp(time, SongStartTime, ReplayEndTime);
            base.Rewind(time, resumeAfterRewind);
            _songReachedReplayEnd = false;
        }

        private void LateUpdate() {
            if (_songReachedReplayEnd || SongTime < ReplayEndTime) return;
            SongReachedReplayEndEvent?.Invoke();
            _songReachedReplayEnd = true;
        }
    }
}
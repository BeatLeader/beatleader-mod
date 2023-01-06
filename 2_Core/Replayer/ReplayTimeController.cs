using BeatLeader.Models;
using IPA.Utilities;
using System;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayTimeController : BeatmapTimeController, IReplayTimeController {
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly GameSongController _songController = null!; 

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
            RefreshSongState();
        }

        private void LateUpdate() {
            if (!_songReachedReplayEnd && Mathf.Approximately(SongTime, ReplayEndTime)) {
                SongReachedReplayEndEvent?.Invoke();
                _songReachedReplayEnd = true;
            }
        }

        private void RefreshSongState() {
            _songController.SetField("_songDidFinish", false);
            _songReachedReplayEnd = false;
        }
    }
}

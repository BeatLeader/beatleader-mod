using BeatLeader.Models;
using System;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayTimeController : BeatmapTimeController, IReplayTimeController {
        [Inject] private readonly StandardLevelGameplayManager.InitData _gameplayManagerInitData = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public float ReplayEndTime {
            get {
                var failTime = !_gameplayManagerInitData.failOn0Energy ? 0 : _launchData.MainReplay.info.failTime;
                return _launchData.IsBattleRoyale || failTime == 0 ? SongEndTime : failTime;
            }
        }

        public event Action? SongReachedReplayEndEvent;

        private bool _songReachedReplayEnd;

        public override void Rewind(float time, bool resumeAfterRewind = true) {
            time = Mathf.Clamp(time, SongStartTime, ReplayEndTime);
            base.Rewind(time, resumeAfterRewind);
            _songReachedReplayEnd = false;
        }

        private void LateUpdate() {
            if (!_songReachedReplayEnd && SongTime >= ReplayEndTime - 0.01f) {
                SongReachedReplayEndEvent?.Invoke();
                _songReachedReplayEnd = true;
            }
        }
    }
}

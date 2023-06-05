using BeatLeader.Models;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    internal class ReplayTimeController : BeatmapTimeController, IReplayTimeController {
        [Inject] private readonly StandardLevelGameplayManager.InitData _gameplayManagerInitData = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public float ReplayEndTime {
            get {
                _replayEndTime = _replayEndTime is -1 ? _launchData.MainReplay.PlayerMovementFrames.LastOrDefault().time : _replayEndTime;
                var failTime = !_gameplayManagerInitData.failOn0Energy ? 0 : _replayEndTime;
                return _launchData.IsBattleRoyale || failTime == 0 ? SongEndTime : failTime;
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
            if (!_songReachedReplayEnd && (Math.Abs(SongTime - ReplayEndTime) < 0.01f || SongTime >= ReplayEndTime)) {
                SongReachedReplayEndEvent?.Invoke();
                _songReachedReplayEnd = true;
            }
        }
    }
}
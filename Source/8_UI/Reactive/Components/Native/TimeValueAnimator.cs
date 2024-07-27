using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal class TimeValueAnimator {
        public float Progress {
            get => _progress;
            private set {
                _progress = float.IsNaN(value) ? 0f : value;
                ProgressChangedEvent?.Invoke(value);
            }
        }

        public event Action<float>? ProgressChangedEvent;

        public float Duration {
            get => _duration;
            set {
                _elapsedTime = MathUtils.Map(_elapsedTime, 0f, Duration, 0f, value);
                _duration = value;
                _progress = _elapsedTime / Duration;
            }
        }

        private bool _set = true;
        private bool _startedThisFrame;
        private float _progress;
        private float _elapsedTime;
        private float _multiplier;
        private float _duration;

        public void Execute(bool reversed = false) {
            _set = false;
            _startedThisFrame = true;
            _multiplier = reversed ? -1f : 1f;
        }

        public void SetProgress(float progress) {
            _set = false;
            _elapsedTime = Duration * progress;
            _progress = progress;
        }

        public void Update() {
            if (_set) return;
            _elapsedTime += Time.deltaTime * _multiplier;
            _elapsedTime = float.IsNaN(_elapsedTime) ? 0f : _elapsedTime;
            Progress = Mathf.Clamp01(_elapsedTime / Duration);
            //finishing
            if (_elapsedTime >= Duration) {
                _set = true;
                _elapsedTime = Duration;
            } else if (_elapsedTime <= 0 && !_startedThisFrame) {
                _set = true;
                _elapsedTime = 0f;
            }
            _startedThisFrame = false;
        }
    }
}
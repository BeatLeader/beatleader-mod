using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal class ValueAnimator {
        public float Progress {
            get => _progress;
            private set {
                _progress = value;
                ProgressChangedEvent?.Invoke(value);
            }
        }

        public float LerpCoefficient { get; set; } = 10f;

        public event Action<float>? ProgressChangedEvent;

        private float _targetValue;
        private bool _set;
        private float _progress;

        public void SetProgress(float progress) {
            _set = false;
            _progress = Mathf.Clamp01(progress);
        }
        
        public void SetTarget(float value) {
            if (Math.Abs(value - _targetValue) < 0.001) return;
            _set = false;
            _targetValue = value;
        }

        public void Push() {
            _targetValue = 1f;
            _set = false;
        }

        public void Pull() {
            _targetValue = 0f;
            _set = false;
        }

        public void Update() {
            if (_set) return;
            if (Math.Abs(_targetValue - Progress) < 1e-6) {
                Progress = _targetValue;
                _set = true;
            } else {
                Progress = Mathf.Lerp(Progress, _targetValue, Time.deltaTime * LerpCoefficient);
            }
        }
    }
}
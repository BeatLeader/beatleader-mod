using System;
using UnityEngine;

namespace BeatLeader {
    internal class ValueAnimator {
        public float Value { get; set; }
        public float LerpCoefficient { get; set; } = 10f;

        private float _targetValue;
        private bool _set = true;

        public void SetTarget(float value) {
            _targetValue = value;
            _set = false;
        }

        public void Update() {
            if (_set) {
                return;
            }
            if (Math.Abs(_targetValue - Value) < 0.001f) {
                Value = _targetValue;
                _set = true;
            } else {
                Value = Mathf.Lerp(Value, _targetValue, Time.deltaTime * LerpCoefficient);
            }
        }
    }
}
using System;
using UnityEngine;

namespace BeatLeader {
    internal class ValueAnimator {
        public Vector3 Value { get; set; }
        public float LerpCoefficient { get; set; } = 10f;
        public bool Set => _set;

        private Vector3 _targetValue;
        private bool _set = true;
        private bool _shouldBeSet = true;

        public void SetTarget(Vector3 value) {
            _targetValue = value;
            _set = false;
            _shouldBeSet = false;
        }

        public void Update() {
            if (_set) {
                return;
            }
            if (Math.Abs(_targetValue.x - Value.x) < 0.001f) {
                Value = _targetValue;
                _shouldBeSet = true;
            } else {
                Value = Vector3.Lerp(Value, _targetValue, Time.deltaTime * LerpCoefficient);
            }
        }

        public void LateUpdate() {
            _set = _shouldBeSet;
        }
    }
}
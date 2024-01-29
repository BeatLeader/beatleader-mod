using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    public class SmoothHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public bool ForceKeepHovered {
            get => _forceKeepHovered;
            set {
                _forceKeepHovered = value;
                _targetValue = value ? 1f : _alternativeTargetValue;
                _set = false;
            }
        }

        public bool IsHovered { get; private set; }
        public float Progress { get; private set; }

        public event Action<bool, float>? HoverStateChangedEvent;

        public float lerpCoefficient = 10.0f;

        private bool _forceKeepHovered;
        private float _targetValue;
        private float _alternativeTargetValue;
        private bool _set;

        private void OnDisable() {
            IsHovered = false;
            Progress = _targetValue = 0.0f;
            _set = false;
            HoverStateChangedEvent?.Invoke(IsHovered, Progress);
        }

        private void Update() {
            if (_set) return;

            if (Math.Abs(_targetValue - Progress) < 1e-6) {
                Progress = _targetValue;
                _set = true;
            } else {
                Progress = Mathf.Lerp(Progress, _targetValue, Time.deltaTime * lerpCoefficient);
            }

            HoverStateChangedEvent?.Invoke(IsHovered, Progress);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (ForceKeepHovered) {
                _alternativeTargetValue = 1f;
                return;
            }
            IsHovered = true;
            _targetValue = 1f;
            _set = false;
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (ForceKeepHovered) {
                _alternativeTargetValue = 0f;
                return;
            }
            IsHovered = false;
            _targetValue = 0f;
            _set = false;
        }
    }
}
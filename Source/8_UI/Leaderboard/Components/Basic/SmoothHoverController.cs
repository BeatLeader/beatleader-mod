using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    public class SmoothHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public event Action<bool, float>? HoverStateChangedEvent;

        public float lerpCoefficient = 10.0f;
        
        public bool IsHovered { get; private set; }
        public float Progress { get; private set; }
        
        private float _targetValue;
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
            IsHovered = true;
            _targetValue = 1.0f;
            _set = false;
        }

        public void OnPointerExit(PointerEventData eventData) {
            IsHovered = false;
            _targetValue = 0.0f;
            _set = false;
        }
    }
}
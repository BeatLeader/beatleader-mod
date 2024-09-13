using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    public class SmoothHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        #region Factory Methods

        public static SmoothHoverController Custom(GameObject gameObject, StateChangedDelegate handler) {
            var component = gameObject.AddComponent<SmoothHoverController>();
            component.AddStateListener(handler);
            return component;
        }

        public static SmoothHoverController Scale(GameObject gameObject, float defaultScale, float hoverScale) {
            return Scale(gameObject, gameObject.transform, defaultScale, hoverScale);
        }

        public static SmoothHoverController Scale(GameObject gameObject, Transform target, float defaultScale, float hoverScale) {
            var component = gameObject.AddComponent<SmoothHoverController>();
            component.AddStateListener(((hovered, progress) => {
                var scale = Mathf.Lerp(defaultScale, hoverScale, progress);
                target.localScale = new Vector3(scale, scale, scale);
            }));
            return component;
        }

        #endregion

        #region OnStateChanged

        public delegate void StateChangedDelegate(bool hovered, float progress);

        private event StateChangedDelegate OnStateChanged;

        public void AddStateListener(StateChangedDelegate handler) {
            OnStateChanged += handler;
            handler?.Invoke(IsHovered, Progress);
        }

        public void RemoveStateListener(StateChangedDelegate handler) {
            OnStateChanged -= handler;
        }

        private void NotifyStateChanged() {
            OnStateChanged?.Invoke(IsHovered, Progress);
        }

        #endregion

        #region Logic

        public float lerpCoefficient = 10.0f;

        public bool IsHovered { get; private set; }
        public float Progress { get; private set; }

        private float _targetValue;
        private bool _set;

        private void OnDisable() {
            IsHovered = false;
            Progress = _targetValue = 0.0f;
            _set = false;
            NotifyStateChanged();
        }

        private void Update() {
            if (_set) return;

            if (Mathf.Abs(_targetValue - Progress) < 1e-6) {
                Progress = _targetValue;
                _set = true;
            } else {
                Progress = Mathf.Lerp(Progress, _targetValue, Time.deltaTime * lerpCoefficient);
            }

            NotifyStateChanged();
        }

        #endregion

        #region Events

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

        #endregion
    }
}
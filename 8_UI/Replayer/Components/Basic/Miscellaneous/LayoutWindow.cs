using BeatLeader.Utils;
using System;
using UnityEngine;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class LayoutWindow : MonoBehaviour {
        public RectTransform Target {
            get => _target;
            set {
                _target = value;
                _targetParent = _target.parent.GetComponent<RectTransform>();
                if (!_useHandle) {
                    ApplyHandleTo(_target.gameObject);
                }
            }
        }
        public RectTransform Handle {
            get => _handle;
            set {
                _handle = value;
                if (_useHandle) {
                    ApplyHandleTo(_handle.gameObject);
                }
                _useHandle = _handle != null;
            }
        }
        public Vector2 LastCursorPos { get; private set; }

        public event Action<Vector2> WindowPositionChangedEvent;

        protected LayoutHandle _layoutHandle;
        protected RectTransform _target;
        protected RectTransform _targetParent;
        protected RectTransform _handle;
        protected Vector2 _movementOffset;
        protected bool _useHandle;
        private bool _allowDrag;

        public void RecalculateImmediate() {
            SetPosition(_target.localPosition);
        }
        public void SetPosition(Vector2 cursorPos) {
            LastCursorPos = cursorPos;
            var appl = ApplyPosition(cursorPos);
            _target.localPosition = appl;
            WindowPositionChangedEvent?.Invoke(appl);
        }
        public void Setup(RectTransform target, RectTransform handle = null) {
            Target = target;
            Handle = handle;
        }

        protected virtual Vector2 ApplyPosition(Vector2 cursorPos) {
            return cursorPos;
        }

        private void ApplyHandleTo(GameObject go) {
            if (_layoutHandle != null) {
                _layoutHandle.HandleDraggingEvent -= OnDrag;
                _layoutHandle.HandleWasGrabbedEvent-= OnBeginDrag;
                _layoutHandle.HandleWasReleasedEvent -= OnEndDrag;
            }
            _layoutHandle.TryDestroy();
            _layoutHandle = go.AddComponent<LayoutHandle>();

            _layoutHandle.HandleDraggingEvent += OnDrag;
            _layoutHandle.HandleWasGrabbedEvent += OnBeginDrag;
            _layoutHandle.HandleWasReleasedEvent += OnEndDrag;
        }
        private bool TransformMousePos(out Vector2 transformedMousePos) {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _targetParent, Input.mousePosition, null, out transformedMousePos);
        }
        private bool MouseOnHandle() {
            return RectTransformUtility.RectangleContainsScreenPoint(_handle, Input.mousePosition);
        }

        private void OnDrag() {
            if (!_allowDrag || !enabled) return;
            TransformMousePos(out Vector2 parsedMousePos);
            SetPosition(parsedMousePos - _movementOffset);
        }
        private void OnBeginDrag() {
            if (_target == null && !enabled) return;
            if (TransformMousePos(out var parsedMousePos) && (!_useHandle || MouseOnHandle())) {
                _movementOffset = parsedMousePos - (Vector2)_target.localPosition;
                _allowDrag = true;
            }
        }
        private void OnEndDrag() {
            _allowDrag = false;
        }
    }
}

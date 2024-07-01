using System;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI {
    [DisallowMultipleComponent]
    internal class FloatingHandle : MonoBehaviour {
        #region Setup

        private VRPointerEventsHandler _pointerEventsHandler = null!;
        private Transform? _screen;
        private bool _isInitialized;

        public void Setup(Transform screen) {
            _screen = screen;
            _isInitialized = true;
        }

        private void Awake() {
            _pointerEventsHandler = gameObject.AddComponent<VRPointerEventsHandler>();
            _pointerEventsHandler.PointerUpdatedEvent += HandlePointerUpdated;
            _pointerEventsHandler.PointerDownEvent += HandlePointerDown;
            _pointerEventsHandler.PointerUpEvent += HandlePointerUp;
        }

        #endregion

        #region Animation

        private Vector3 _targetScale = Vector3.zero;
        private bool _hovered;
        private bool _visible;

        public void Present() {
            _pointerEventsHandler.enabled = true;
            _visible = true;
            RefreshTargetScale();
        }

        public void Hide() {
            _pointerEventsHandler.enabled = false;
            _visible = false;
            RefreshTargetScale();
        }

        private void RefreshTargetScale() {
            _targetScale = Vector3.one * (_visible ? _hovered ? 1.2f : 1f : 0f);
        }

        private void Update() {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * 10f);
        }

        #endregion

        #region Movements

        public event Action<Pose>? PoseChangedEvent; 
        
        public bool lookAtCenterPoint;
        public Vector3 centerPoint;

        private Vector3 _lookAtAdjust = new(0f, 180f, 0);
        private float _posCoefficient = 20f;
        private float _rotCoefficient = 15f;

        private float _grabDistance;
        private Vector3 _grabPointDelta;
        private Quaternion _grabRot;
        private VRController? _grabbingController;

        private void LateUpdate() {
            if (!_isInitialized || _grabbingController == null) return;
            //distance
            _grabDistance -= _grabbingController.verticalAxisValue * Time.deltaTime;
            //position
            var controllerTransform = _grabbingController.transform;
            var newPos = controllerTransform.position + controllerTransform.forward * _grabDistance + _grabPointDelta;
            _screen!.position = Vector3.Lerp(_screen!.position, newPos, _posCoefficient * Time.deltaTime);
            //rotation
            Quaternion rot;
            if (lookAtCenterPoint) {
                var worldPos = _screen.parent.TransformPoint(centerPoint);
                rot = Quaternion.LookRotation(worldPos - _screen.position);
                rot *= Quaternion.Euler(_lookAtAdjust);
            } else {
                rot = controllerTransform.rotation * _grabRot;
            }
            var handlePos = transform.position;
            _screen.rotation = Quaternion.Slerp(_screen.rotation, rot, _rotCoefficient * Time.deltaTime);
            // position updates after the screen rotation so suppressing the warning
            // ReSharper disable once Unity.InefficientPropertyAccess
            _grabPointDelta -= transform.position - handlePos;
        }

        #endregion

        #region Callbacks

        private void HandlePointerUpdated(VRPointerEventsHandler handler, RaycastHit hit) {
            _hovered = handler.IsHovered || handler.IsPressed;
            RefreshTargetScale();
        }

        private void HandlePointerDown(VRPointerEventsHandler handler, RaycastHit hit) {
            if (!_isInitialized) return;
            _grabbingController = handler.VRController;
            //
            var controllerTransform = _grabbingController!.transform;
            _grabDistance = Vector3.Distance(controllerTransform.position, hit.point);
            _grabPointDelta = _screen!.position - hit.point;
            _grabRot = Quaternion.Inverse(controllerTransform.rotation) * _screen.rotation;
        }

        private void HandlePointerUp(VRPointerEventsHandler handler, RaycastHit hit) {
            _grabbingController = null;
            PoseChangedEvent?.Invoke(_screen!.transform.GetLocalPose());
        }

        #endregion
    }
}
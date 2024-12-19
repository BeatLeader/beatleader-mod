using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;

namespace BeatLeader {
    public class ChristmasTreeMover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {
        private static readonly int treePositionPropertyId = Shader.PropertyToID("_TreePosition");
        private static readonly int textureTintId = Shader.PropertyToID("_TextureTint");

        [SerializeField] private Transform _container = null!;
        [SerializeField] private Material _material = null!;

        private bool _hovered;
        private bool _grabbed;

        private Transform? _grabbingController;
        private VRController? _grabbingVRController;
        private ReeTransform _attachmentLocalPose;
        private Vector3 _attachmentLocalUp;
        private ReeTransform _grabWorldPose;
        private Quaternion _grabRotation;
        private Vector3 _grabScale;
        private float _rotOffset;

        private readonly ValueAnimator _highlightAnimator = new ValueAnimator();
        private readonly ValueAnimator _scaleAnimator = new ValueAnimator();

        private bool _full;
        private bool _restricted;

        public void SetEnabled(bool full, bool restricted) {
            _full = full;
            _restricted = restricted;
            _scaleAnimator.SetTarget(full || restricted ? 1f : 0f);
        }

        private void RefreshHighlight() {
            _highlightAnimator.SetTarget(_hovered ? _grabbed ? 0.8f : 0.3f : 0);
        }

        private void RefreshColor() {
            _highlightAnimator.Update();
            var color = Color.Lerp(Color.white, Color.red, _highlightAnimator.Value);
            _material.SetColor(textureTintId, color);
        }

        private void RefreshScale() {
            _scaleAnimator.Update();
            transform.localScale = _scaleAnimator.Value * Vector3.one;
        }

        private void Update() {
            var shaderPos = _container.position;
            shaderPos.y += 1.5f;
            Shader.SetGlobalVector(treePositionPropertyId, shaderPos);

            RefreshColor();
            RefreshScale();

            if (!_grabbed) return;

            var controllerPose = ReeTransform.FromTransform(_grabbingController);

            var targetWorldPose = new ReeTransform(
                controllerPose.LocalToWorldPosition(_attachmentLocalPose.Position),
                controllerPose.LocalToWorldRotation(_attachmentLocalPose.Rotation)
            );

            var t = Time.unscaledDeltaTime * 10f;

            // Rotation with X axis
            var stickVec = _grabbingVRController!.thumbstick * t;
            _rotOffset -= stickVec.x * 20f;
            var currentLocalUp = controllerPose.WorldToLocalDirection(Vector3.up);
            var angleDiff = Vector3.SignedAngle(_attachmentLocalUp, currentLocalUp, Vector3.forward);
            var rot = Quaternion.AngleAxis(_rotOffset - angleDiff, Vector3.up) * _grabRotation;

            // Scale
            var scale = _grabScale;
            scale *= targetWorldPose.Position.y / _grabWorldPose.Position.y;
            scale.x = scale.y = scale.z = Mathf.Clamp(scale.y, 0.3f, 2.0f);

            // Pos
            var pos = targetWorldPose.Position;
            pos.y = 0.0f;

            if (_full) {
                _container.position = Vector3.Lerp(_container.position, pos, t);
            }

            if (_full || _restricted) {
                _container.localScale = Vector3.Lerp(_container.localScale, scale, t);
                _container.rotation = Quaternion.Lerp(_container.rotation, rot, t);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!_hovered || eventData.currentInputModule is not VRInputModule module) {
                return;
            }

            _grabbingVRController = module.vrPointer.lastSelectedVrController;
            _grabbingController = _grabbingVRController.transform;

            var controllerPose = ReeTransform.FromTransform(_grabbingController);

            _attachmentLocalPose = new ReeTransform(
                controllerPose.WorldToLocalPosition(transform.position),
                controllerPose.WorldToLocalRotation(transform.rotation)
            );

            _attachmentLocalUp = controllerPose.WorldToLocalDirection(Vector3.up);

            _grabWorldPose = new ReeTransform(
                transform.position,
                transform.rotation
            );

            _rotOffset = 0.0f;
            _grabScale = _container.localScale;
            _grabRotation = _container.rotation;

            _grabbed = true;
            RefreshHighlight();
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (!_grabbed) {
                return;
            }

            _grabbingController = null;
            _grabbingVRController = null;
            _grabbed = false;
            RefreshHighlight();
        }

        public void OnPointerExit(PointerEventData eventData) {
            _hovered = false;
            RefreshHighlight();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _hovered = true;
            RefreshHighlight();
        }
    }
}
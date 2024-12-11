using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;

namespace BeatLeader {
    public class ChristmasTreeMover : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {
        private static readonly int textureTintId = Shader.PropertyToID("_TextureTint");
        
        [SerializeField] private Transform _container = null!;
        [SerializeField] private Material _material = null!;

        private bool _hovered;
        private bool _grabbed;

        private Transform? _grabbingController;
        private VRController? _grabbingVRController;
        private Vector3 _grabPos;
        private Quaternion _grabRot;

        private readonly ValueAnimator _highlightAnimator = new();
        private readonly ValueAnimator _scaleAnimator = new();

        public void SetEnabled(bool enable) {
            _scaleAnimator.SetTarget(enable ? 1f : 0f);
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
            RefreshColor();
            RefreshScale();
            if (!_grabbed) {
                return;
            }
            var t = Time.unscaledDeltaTime;
            var stickVec = _grabbingVRController!.thumbstick * t;
            
            _grabPos += Vector3.forward * (stickVec.y * 5f);
            var pos = _grabbingController!.TransformPoint(_grabPos);
            pos.y = 0;
            _container.position = Vector3.Lerp(_container.position, pos, t * 5f);

            var initialRot = _container.eulerAngles;
            initialRot.y -= stickVec.x * 50f;
            _container.eulerAngles = initialRot;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!_hovered || eventData.currentInputModule is not VRInputModule module) {
                return;
            }
            _grabbingVRController = module.vrPointer.lastSelectedVrController;
            _grabbingController = _grabbingVRController.transform;
            _grabPos = _grabbingController.InverseTransformPoint(_container.position);
            _grabRot = Quaternion.Inverse(_grabbingController.rotation) * _container.rotation;
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
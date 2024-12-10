using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VRUIControls;

namespace BeatLeader.Components {
    [RequireComponent(typeof(Rigidbody))]
    internal class ChristmasTreeOrnament : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler {
        #region Setup

        public int BundleId { get; private set; }

        public event Action<ChristmasTreeOrnament>? OrnamentDeinitEvent;
        public event Action<ChristmasTreeOrnament>? OrnamentGrabbedEvent;

        private Rigidbody _rigidbody = null!;
        private ChristmasTree? _tree;
        private bool _initialized;

        public void Setup(ChristmasTree tree, int bundleId) {
            _tree = tree;
            BundleId = bundleId;
            _initialized = true;
        }

        public void Init(Transform parent) {
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            gameObject.SetActive(true);
        }

        private void Deinit() {
            _rigidbody.useGravity = false;
            _rigidbody.velocity = Vector3.zero;
            _hadContact = false;
            _grabbed = false;
            _hovered = false;
            gameObject.SetActive(false);
        }

        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        #endregion

        #region Input

        private bool _hadContact;
        private bool _hovered;
        private bool _grabbed;

        private Transform? _grabbingController;
        private Vector3 _alignedOrnamentPos;
        private Vector3 _grabPos;
        private Quaternion _grabRot;

        private void Update() {
            if (!_initialized) {
                return;
            }
            if (!_grabbed) {
                if (_hadContact) {
                    var t = Time.deltaTime * 7f;
                    transform.localPosition = Vector3.Lerp(transform.localPosition, _alignedOrnamentPos, t);
                } else if (transform.position.y < 0) {
                    Deinit();
                    OrnamentDeinitEvent?.Invoke(this);
                }
            } else {
                transform.position = _grabbingController!.TransformPoint(_grabPos);
                transform.rotation = _grabbingController.rotation * _grabRot;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!_hovered || eventData.currentInputModule is not VRInputModule module) {
                return;
            }
            _grabbingController = module.vrPointer.lastSelectedVrController.transform;
            _grabPos = _grabbingController.InverseTransformPoint(transform.position);
            _grabRot = Quaternion.Inverse(_grabbingController.rotation) * transform.rotation;
            _rigidbody.useGravity = false;
            _grabbed = true;
            OrnamentGrabbedEvent?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (!_grabbed) {
                return;
            }
            _grabbingController = null;
            //TODO: implement alignment
            _hadContact = _tree!.HasAreaContact(transform.position);
            _rigidbody.useGravity = !_hadContact;
            _grabbed = false;
            if (_hadContact) {
                transform.SetParent(_tree!.TreeMesh, true);
                _alignedOrnamentPos = transform.localPosition;
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            _hovered = false;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _hovered = true;
        }

        #endregion
    }
}
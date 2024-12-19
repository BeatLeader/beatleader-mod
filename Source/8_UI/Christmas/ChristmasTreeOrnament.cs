using System;
using BeatLeader.Models;
using BeatLeader.Utils;
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

        public ChristmasTreeOrnamentSettings GetSettings() {
            return new ChristmasTreeOrnamentSettings {
                bundleId = BundleId,
                pose = transform.GetLocalPose()
            };
        }

        public void Init() {
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
            _rigidbody.interpolation = RigidbodyInterpolation.None;
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
                    var pos = Vector3.Lerp(transform.localPosition, _alignedOrnamentPos, t);
                    if (pos.x - _alignedOrnamentPos.x < 0.001f) {
                        pos = _alignedOrnamentPos;
                        _hadContact = false;
                    }

                    transform.localPosition = pos;
                } else if (transform.position.y < 0) {
                    Deinit();
                    OrnamentDeinitEvent?.Invoke(this);
                }
            } else {
                transform.position = _grabbingController!.TransformPoint(_grabPos);
                transform.rotation = _grabbingController.rotation * _grabRot;
            }
        }

        private const float MaxDistance = 0.17f;

        public void OnPointerDown(PointerEventData eventData) {
            if (!_hovered || eventData.currentInputModule is not VRInputModule module) {
                return;
            }

            _grabbingController = module.vrPointer.lastSelectedVrController.transform;
            _grabPos = _grabbingController.InverseTransformPoint(transform.position);
            if (_grabPos.magnitude > MaxDistance) {
                _grabPos = _grabPos.normalized * MaxDistance;
            }

            _grabRot = Quaternion.Inverse(_grabbingController.rotation) * transform.rotation;
            _rigidbody.useGravity = false;
            _grabbed = true;
            
            transform.SetParent(_tree!.Origin, true);
            transform.localScale = Vector3.one;
            
            OrnamentGrabbedEvent?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (!_grabbed) {
                return;
            }

            _grabbingController = null;
            //TODO: implement alignment
            _hadContact = _tree!.HasAreaContact(transform.position);
            if (!_hadContact) {
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.useGravity = true;
            }

            _grabbed = false;

            if (_hadContact) {
                transform.SetParent(_tree!.Origin, true);
                _alignedOrnamentPos = transform.localPosition;
                _tree.AddOrnament(this);
            } else {
                _tree.RemoveOrnament(this);
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
using System;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.Components {
    internal class GrabbingInputHelper {
        #region Controller

        private class GrabbingController : MonoBehaviour {
            public event Action<Transform, Transform>? ItemGrabbedEvent;
            public event Action<Transform, Transform>? ItemReleasedEvent;

            private VRController _controller = null!;
            private Transform? _contactingTransform;
            private bool _hasContact;
            private bool _grabbed;

            private void Awake() {
                var collider = gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = GetComponent<MeshFilter>().mesh;
                _controller = GetComponentInParent<VRController>();
            }

            private void Update() {
                var pressed = _controller.triggerValue < 0.1f;
                if (!_grabbed && pressed && _hasContact) {
                    _grabbed = true;
                    ItemGrabbedEvent?.Invoke(_contactingTransform!, _controller.transform);
                } else if (_grabbed && (!pressed || !_hasContact)) {
                    _grabbed = false;
                    ItemReleasedEvent?.Invoke(_contactingTransform!, _controller.transform);
                }
            }

            private void OnCollisionEnter(Collision other) {
                if (!_grabbed) {
                    _contactingTransform = other.transform;
                    _hasContact = true;
                }
            }

            private void OnCollisionExit(Collision other) {
                if (_grabbed && other.transform == _contactingTransform) {
                    _contactingTransform = null;
                    _hasContact = false;
                }
            }
        }

        #endregion

        public GrabbingInputHelper(VRInputModule inputModule, MenuPlayerController playerController) {
            _inputModule = inputModule;
            _leftController = AddGrabbingController(playerController._leftController.transform);
            _rightController = AddGrabbingController(playerController._rightController.transform);
            Enabled = false;
        }

        public bool Enabled {
            get => _enabled;
            set {
                _inputModule.gameObject.SetActive(!value);
                _leftController.enabled = value;
                _rightController.enabled = value;
                _enabled = value;
            }
        }

        public event Action<Transform, Transform>? ItemGrabbedEvent;
        public event Action<Transform, Transform>? ItemReleasedEvent;

        private readonly VRInputModule _inputModule;
        private readonly GrabbingController _leftController;
        private readonly GrabbingController _rightController;
        private bool _enabled;

        private GrabbingController AddGrabbingController(Transform transform) {
            var handle = transform.Find("MenuHandle/Glowing").gameObject;
            var comp = handle.AddComponent<GrabbingController>();
            comp.ItemGrabbedEvent += HandleGrabbed;
            comp.ItemReleasedEvent += HandleReleased;
            return comp;
        }

        private void HandleGrabbed(Transform item, Transform controller) {
            ItemGrabbedEvent?.Invoke(item, controller);
        }

        private void HandleReleased(Transform item, Transform controller) {
            ItemReleasedEvent?.Invoke(item, controller);
        }
    }
}
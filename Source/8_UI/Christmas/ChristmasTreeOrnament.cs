using System;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ChristmasTreeOrnament : MonoBehaviour {
        #region Setup

        public int BundleId { get; private set; }

        public event Action<ChristmasTreeOrnament>? OrnamentDeinitEvent;
        public event Action<ChristmasTreeOrnament>? OrnamentGrabbedEvent;

        private Rigidbody _rigidbody = null!;
        private GrabbingInputHelper? _inputHelper;
        private ChristmasTree? _tree;
        private bool _initialized;

        public void Setup(GrabbingInputHelper inputHelper, ChristmasTree tree) {
            _tree = tree;
            _inputHelper = inputHelper;
        }
        
        public void Init(int bundleId) {
            BundleId = bundleId;
            
            transform.SetParent(_tree!.transform, false);
            _inputHelper!.ItemGrabbedEvent += HandleItemGrabbed;
            _inputHelper.ItemReleasedEvent += HandleItemReleased;
            
            _initialized = true;
            gameObject.SetActive(true);
        }

        private void Deinit() {
            _inputHelper!.ItemGrabbedEvent -= HandleItemGrabbed;
            _inputHelper.ItemReleasedEvent -= HandleItemReleased;
            gameObject.SetActive(false);
        }

        private void Awake() {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
            _rigidbody.useGravity = true;
        }

        #endregion

        #region Input

        private bool _hasAreaContact;
        private bool _grabbed;

        private Transform? _grabbingController;
        private Vector3 _alignedOrnamentPos;
        private Vector3 _grabOffset;

        private void Update() {
            if (!_initialized) {
                return;
            }
            if (!_grabbed) 
            {
                if (_hasAreaContact)
                {
                    var t = Time.deltaTime * 7f;
                    transform.localPosition = Vector3.Lerp(transform.localPosition, _alignedOrnamentPos, t);
                } 
                else if (transform.position.y < 0)
                {
                    OrnamentDeinitEvent?.Invoke(this);
                    Deinit();
                }
            }
            else 
            {
                transform.position = _grabbingController!.position + _grabOffset;
            }
        }

        private void OnCollisionEnter(Collision other) {
            if (!_initialized) {
                return;
            }
            if (other.collider == _tree!.AreaCollider) {
                _hasAreaContact = true;
            }
        }

        private void OnCollisionExit(Collision other) {
            if (!_initialized) {
                return;
            }
            if (other.collider != _tree!.AreaCollider) {
                _hasAreaContact = false;
            }
        }

        private void HandleItemGrabbed(Transform self, Transform controller) {
            if (self != transform || _grabbed) {
                return;
            }
            _grabbingController = controller;
            _grabOffset = transform.position - controller.position;
            _rigidbody.useGravity = false;
            _grabbed = true;
            OrnamentGrabbedEvent?.Invoke(this);
        }

        private void HandleItemReleased(Transform self, Transform controller) {
            if (self != transform || controller != _grabbingController || !_grabbed) {
                return;
            }
            _grabbingController = null;
            _alignedOrnamentPos = _tree!.Align(transform.position);
            _rigidbody.useGravity = !_hasAreaContact;
            _grabbed = false;
        }

        #endregion
    }
}
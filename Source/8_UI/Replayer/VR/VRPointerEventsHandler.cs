using System;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.UI {
    internal class VRPointerEventsHandler : MonoBehaviour {
        #region Events

        public event Action<VRPointerEventsHandler, RaycastHit>? PointerUpdatedEvent;
        public event Action<VRPointerEventsHandler, RaycastHit>? PointerDownEvent;
        public event Action<VRPointerEventsHandler, RaycastHit>? PointerUpEvent;
        public event Action<VRPointerEventsHandler, RaycastHit>? PointerEnterEvent;
        public event Action<VRPointerEventsHandler, RaycastHit>? PointerExitEvent;

        #endregion

        #region Setup

        public VRController? VRController { get; private set; }
        public bool IsHovered { get; private set; }
        public bool IsPressed { get; private set; }

        public float raycastDistanceThreshold = 50f;

        private VRPointer _pointer = null!;

        private void Awake() {
            _pointer = FindObjectOfType<VRPointer>();
        }

        #endregion

        #region Handling

        private void Update() {
            var controller = _pointer.vrController;
            var hovered = Physics.Raycast(
                controller.position,
                controller.forward,
                out var hitInfo,
                raycastDistanceThreshold
            ) && hitInfo.transform == transform;
            //
            var triggerPressed = controller.triggerValue >= 0.9f;
            //if was pressed previous frame and still is pressed keep treating as pressed
            var pressed = (hovered && triggerPressed) || (triggerPressed && IsPressed);
            //settings values
            VRController = pressed || hovered ? controller : null;
            //invoking hover events
            var needToUpdateHover = IsHovered != hovered;
            var needToUpdatePress = IsPressed != pressed;
            IsHovered = hovered;
            IsPressed = pressed;
            //invoking hover events
            if (needToUpdateHover) {
                if (hovered) {
                    PointerEnterEvent?.Invoke(this, hitInfo);
                } else {
                    PointerExitEvent?.Invoke(this, hitInfo);
                }
            }
            //invoking press events
            if (needToUpdatePress) {
                if (pressed) {
                    PointerDownEvent?.Invoke(this, hitInfo);
                } else {
                    PointerUpEvent?.Invoke(this, hitInfo);
                }
            }
            //invoking pointer updated event
            if (needToUpdateHover || needToUpdatePress) {
                PointerUpdatedEvent?.Invoke(this, hitInfo);
            }
        }

        #endregion
    }
}
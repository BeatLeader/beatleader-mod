using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class LayoutHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
        public event Action? HandleWasGrabbedEvent;
        public event Action? HandleWasReleasedEvent;
        public event Action? HandleDraggingEvent;

        public void OnEndDrag(PointerEventData data) {
            HandleWasReleasedEvent?.Invoke();
        }
        public void OnBeginDrag(PointerEventData data) {
            HandleWasGrabbedEvent?.Invoke();
        }
        public void OnDrag(PointerEventData data) {
            HandleDraggingEvent?.Invoke();
        }
    }
}

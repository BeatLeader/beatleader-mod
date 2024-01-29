using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class PointerEventsHandler : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IDragHandler,
        IBeginDragHandler,
        IEndDragHandler {
        #region Events

        public event Action<PointerEventsHandler, PointerEventData>? PointerUpdatedEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerDownEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerUpEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerEnterEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerExitEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerDragEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerDragBeginEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerDragEndEvent;

        #endregion

        #region Helpers

        public bool IsFocused => IsDragging || IsPressed || IsHovered;
        public bool IsPressed { get; private set; }
        public bool IsHovered { get; private set; }
        public bool IsDragging { get; private set; }
        
        private void NotifyPointerUpdated(PointerEventData data) {
            PointerUpdatedEvent?.Invoke(this, data);
        }

        #endregion

        #region Callbacks

        public void OnPointerDown(PointerEventData eventData) {
            IsPressed = true;
            PointerDownEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            IsPressed = false;
            PointerUpEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            IsHovered = true;
            PointerEnterEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {
            IsHovered = false;
            PointerExitEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            PointerDragEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            IsDragging = true;
            PointerDragBeginEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnEndDrag(PointerEventData eventData) {
            IsDragging = false;
            PointerDragEndEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        #endregion
    }
}
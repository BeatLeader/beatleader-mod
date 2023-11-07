using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class PointerEventsHandler : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler {
        
        #region Events

        public event Action<PointerEventsHandler, PointerEventData>? PointerDownEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerUpEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerEnterEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerExitEvent;

        #endregion

        #region Callbacks

        public void OnPointerDown(PointerEventData eventData) {
            PointerDownEvent?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            PointerUpEvent?.Invoke(this, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            PointerEnterEvent?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {
            PointerExitEvent?.Invoke(this, eventData);
        }

        #endregion
    }
}
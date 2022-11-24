using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    [RequireComponent(typeof(RectTransform))]
    internal class LayoutHandle : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler {
        public event Action DragEvent;
        public event Action BeginDragEvent;
        public event Action EndDragEvent;

        public void OnEndDrag(PointerEventData data) {
            EndDragEvent?.Invoke();
        }
        public void OnBeginDrag(PointerEventData data) {
            BeginDragEvent?.Invoke();
        }
        public void OnDrag(PointerEventData data) {
            DragEvent?.Invoke();
        }
    }
}

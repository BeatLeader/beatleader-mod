using System;
using UnityEngine.EventSystems;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class InteractionEventsProvider : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<PointerEventData> PointerClickEvent;
        public event Action<PointerEventData> PointerDownEvent;
        public event Action<PointerEventData> PointerUpEvent;
        public event Action<PointerEventData> PointerEnterEvent;
        public event Action<PointerEventData> PointerLeaveEvent;

        public event Action<PointerEventData> DragEvent;
        public event Action<PointerEventData> BeginDragEvent;
        public event Action<PointerEventData> EndDragEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClickEvent?.Invoke(eventData);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownEvent?.Invoke(eventData);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpEvent?.Invoke(eventData);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnterEvent?.Invoke(eventData);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            PointerLeaveEvent?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragEvent?.Invoke(eventData);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            BeginDragEvent?.Invoke(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            EndDragEvent?.Invoke(eventData);
        }
    }
}

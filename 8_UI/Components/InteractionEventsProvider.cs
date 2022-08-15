using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class InteractionEventsProvider : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public event Action<PointerEventData> OnPointerClickEvent;
        public event Action<PointerEventData> OnPointerDownEvent;
        public event Action<PointerEventData> OnPointerUpEvent;
        public event Action<PointerEventData> OnPointerEnterEvent;
        public event Action<PointerEventData> OnPointerLeaveEvent;

        public event Action<PointerEventData> OnDragEvent;
        public event Action<PointerEventData> OnBeginDragEvent;
        public event Action<PointerEventData> OnEndDragEvent;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnPointerClickEvent?.Invoke(eventData);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownEvent?.Invoke(eventData);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent?.Invoke(eventData);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEnterEvent?.Invoke(eventData);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerLeaveEvent?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData);
        }
    }
}

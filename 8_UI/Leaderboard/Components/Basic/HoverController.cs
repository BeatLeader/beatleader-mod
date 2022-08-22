using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    public class HoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public event Action<bool> HoverStateChangedEvent;

        private bool _isHovered;

        public bool IsHovered {
            get => _isHovered;
            set {
                if (_isHovered == value) return;
                _isHovered = value;
                HoverStateChangedEvent?.Invoke(value);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            IsHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            IsHovered = false;
        }
    }
}
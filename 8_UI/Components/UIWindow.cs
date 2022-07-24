using BeatLeader.Utils;
using UnityEngine.EventSystems;
using UnityEngine;

namespace BeatLeader.Components
{
    [RequireComponent(typeof(RectTransform))]
    internal class UIWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public RectTransform handle;

        private RectTransform _rect;
        private Vector2 _movementOffset;
        private bool _allowDrag;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
        }
        public void OnEndDrag(PointerEventData data)
        {
            _allowDrag = false;
        }
        public void OnBeginDrag(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, Input.mousePosition, null, out Vector2 result)
                && RectTransformUtility.RectangleContainsScreenPoint(handle, Input.mousePosition))
            {
                _movementOffset = _rect.TransformVector(result * -1);
                _allowDrag = true;
            }
        }
        public void OnDrag(PointerEventData data)
        {
            if (!_allowDrag) return;
            transform.position = (Vector2)Input.mousePosition + _movementOffset;
        }
    }
}

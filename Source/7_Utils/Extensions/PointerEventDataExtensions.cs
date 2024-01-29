using HMUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Utils {
    public static class PointerEventDataExtensions {
        public static Vector2 TranslateToLocalPoint(
            this PointerEventData eventData,
            RectTransform rectTransform,
            Canvas canvas,
            CurvedCanvasSettings? curvedCanvasSettings
        ) {
            var canvasTransform = canvas.transform;
            Vector2 vector;
            if (canvas.renderMode is not RenderMode.WorldSpace) {
                vector = eventData.position;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    vector,
                    canvas.worldCamera!,
                    out vector
                );
            } else {
                vector = eventData.pointerCurrentRaycast.worldPosition;
                vector = canvasTransform.InverseTransformPoint(vector);
                vector = TransformPointFrom3DToCanvas(vector, curvedCanvasSettings?.radius ?? 0);
                vector = canvasTransform.TransformPoint(vector);
                vector = rectTransform.InverseTransformPoint(vector);
            }
            return vector;
        }
        
        private static Vector2 TransformPointFrom3DToCanvas(Vector3 point, float canvasRadius) {
            if (canvasRadius < 1E-10f) return point;
            return new Vector2(Mathf.Asin(point.x / canvasRadius) * canvasRadius, point.y);
        }
    }
}
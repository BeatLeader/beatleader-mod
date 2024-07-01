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
                var worldVector = eventData.pointerCurrentRaycast.worldPosition;
                var canvasVector = canvasTransform.InverseTransformPoint(worldVector);
                canvasVector = TransformPointFrom3DToCanvas(canvasVector, curvedCanvasSettings?.radius ?? 0);
                //very important not to loose z coordinate while translating from canvas to rect
                //spent two evenings to find the issue, please don't be like me :pepega:
                worldVector = canvasTransform.TransformPoint(canvasVector);
                vector = rectTransform.InverseTransformPoint(worldVector);
            }
            return vector;
        }
        
        private static Vector2 TransformPointFrom3DToCanvas(Vector3 point, float canvasRadius) {
            if (canvasRadius < 1E-10f) return point;
            return new Vector2(Mathf.Asin(point.x / canvasRadius) * canvasRadius, point.y);
        }
    }
}
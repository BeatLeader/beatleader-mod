using UnityEngine;

namespace BeatLeader {
    public class ScreenViewTransform {
        public Rect Screen;
        public Rect View;
        
        public ScreenViewTransform(Rect screen, Rect view) {
            Screen = screen;
            View = view;
        }

        #region Normalized

        public Vector2 NormalizeScreenPosition(Vector2 screenPoint) {
            return GetNormalizedPositionUnclamped(Screen, screenPoint);
        }

        public Vector2 NormalizeViewPosition(Vector2 viewPoint) {
            return GetNormalizedPositionUnclamped(View, viewPoint);
        }

        private static Vector2 GetNormalizedPositionUnclamped(Rect rect, Vector2 point) {
            return new Vector2(
                (point.x - rect.xMin) / rect.width,
                (point.y - rect.yMin) / rect.height
            );
        }

        #endregion

        #region TransformPoint

        public Vector2 TransformPoint(Vector2 screenPoint) {
            return FromToPoint(Screen, View, screenPoint);
        }

        public Vector2 InverseTransformPoint(Vector2 viewPoint) {
            return FromToPoint(View, Screen, viewPoint);
        }

        private static Vector2 FromToPoint(Rect from, Rect to, Vector2 point) {
            var normalized = Rect.PointToNormalized(from, point);
            return Rect.NormalizedToPoint(to, normalized);
        }

        #endregion

        #region TransformVector

        public Vector2 TransformVector(Vector2 screenVector) {
            return FromToVector(Screen, View, screenVector);
        }

        public Vector2 InverseTransformVector(Vector2 viewVector) {
            return FromToVector(View, Screen, viewVector);
        }

        private static Vector2 FromToVector(Rect from, Rect to, Vector2 vector) {
            var scale = to.size / from.size;
            return vector * scale;
        }

        #endregion

        #region TransformDirection

        public Vector2 TransformDirection(Vector2 screenDirection) {
            return FromToDirection(Screen, View, screenDirection);
        }

        public Vector2 InverseTransformDirection(Vector2 viewDirection) {
            return FromToDirection(View, Screen, viewDirection);
        }

        private static Vector2 FromToDirection(Rect from, Rect to, Vector2 direction) {
            var vector = FromToVector(from, to, direction);
            vector.Normalize();
            return vector;
        }

        #endregion
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader {
    internal static class AccuracyGraphUtils {
        #region PostProcessPoints

        private const float MinimalXForScaling = 0.05f;

        public static void PostProcessPoints(float[] points, out List<Vector2> positions, out Rect viewRect) {
            positions = new List<Vector2>();

            var yMin = float.MaxValue;
            var yMax = float.MinValue;

            for (var i = 0; i < points.Length; i++) {
                var x = (float) i / (points.Length - 1);
                var y = points[i];
                positions.Add(new Vector2(x, y));
                if (x < MinimalXForScaling) continue;
                if (y > yMax) yMax = y;
                if (y < yMin) yMin = y;
            }

            var margin = (yMax - yMin) * 0.2f;
            viewRect = Rect.MinMaxRect(-0.04f, yMin - margin, 1.04f, yMax + margin);

            ReducePositionsList(positions, viewRect);
        }

        #endregion

        #region ReducePositionsList

        private const float ReduceAngleMargin = 10f;
        private const float ReduceProximityMargin = 0.1f;

        private static void ReducePositionsList(IList<Vector2> positions, Rect viewRect) {
            var startIndex = 1;
            while (startIndex < positions.Count - 1) {
                var i = startIndex;
                for (; i < positions.Count - 1; i++) {
                    var prev = Rect.PointToNormalized(viewRect, positions[i - 1]);
                    var curr = Rect.PointToNormalized(viewRect, positions[i]);
                    var next = Rect.PointToNormalized(viewRect, positions[i + 1]);

                    var a = next - curr;
                    var b = curr - prev;
                    if (a.magnitude > ReduceProximityMargin || b.magnitude > ReduceProximityMargin) continue;
                    if (Vector2.Angle(a, b) > ReduceAngleMargin) continue;
                    positions.RemoveAt(i);
                    break;
                }

                startIndex = i;
            }
        }

        #endregion

        #region TransformPointFrom3DToCanvas

        public static Vector2 TransformPointFrom3DToCanvas(Vector3 point, float canvasRadius) {
            var x = Mathf.Asin(point.x / canvasRadius) * canvasRadius;
            return new Vector2(x, point.y);
        }

        #endregion
    }
}
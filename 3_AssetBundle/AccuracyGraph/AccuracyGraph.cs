using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    public class AccuracyGraph : Graphic {
        #region Serialized

        [SerializeField] private int resolution = 20;
        [SerializeField] private float thickness = 0.1f;

        #endregion

        #region Start

        private GraphMeshHelper _graphMeshHelper;

        protected override void Start() {
            base.Start();
            _graphMeshHelper = new GraphMeshHelper(resolution, 1, thickness);
        }

        #endregion

        #region OnPopulateMesh

        protected override void OnPopulateMesh(VertexHelper vh) {
            var screenRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);
            var screenViewTransform = new ScreenViewTransform(screenRect, _viewRect);

            _graphMeshHelper.SetPoints(_points);
            _graphMeshHelper.PopulateMesh(vh, screenViewTransform, _canvasRadius);
        }

        #endregion

        #region SetPoints

        private List<Vector2> _points;
        private float _canvasRadius;
        private Rect _viewRect = Rect.MinMaxRect(0, 0, 1, 1);

        public void SetPoints(float[] points, float canvasRadius) {
            PostProcessPoints(points, out _points, out _viewRect);
            _canvasRadius = canvasRadius;
            SetVerticesDirty();
        }

        #endregion

        #region PostProcessPoints

        private static void PostProcessPoints(float[] points, out List<Vector2> positions, out Rect viewRect) {
            positions = new List<Vector2>();

            var yMin = float.MaxValue;
            var yMax = float.MinValue;

            for (var i = 0; i < points.Length; i++) {
                var x = (float) i / (points.Length - 1);
                var y = points[i];
                if (y > yMax) yMax = y;
                if (y < yMin) yMin = y;
                positions.Add(new Vector2(x, y));
            }

            ReducePositionsList(positions);
            
            var margin = (yMax - yMin) * 0.1f;
            viewRect = Rect.MinMaxRect(0, yMin - margin, 1, yMax + margin);
        }

        #endregion

        #region ReducePositionsList

        private const float ReduceAngleMargin = 5f;
        private const float ReduceProximityMargin = 0.1f;

        private static void ReducePositionsList(IList<Vector2> positions) {
            var startIndex = 1;
            while (startIndex < positions.Count - 1) {
                var i = startIndex;
                for (; i < positions.Count - 1; i++) {
                    var prev = positions[i - 1];
                    var curr = positions[i];
                    var next = positions[i + 1];

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
    }
}
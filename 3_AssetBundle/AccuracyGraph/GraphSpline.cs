using UnityEngine;

namespace BeatLeader {
    public class GraphSpline {
        #region Construct

        private readonly CyclicBuffer<GraphSplineSegment> _segments;
        private readonly CyclicBuffer<Vector2> _handles;

        public GraphSpline(int capacity) {
            _segments = new CyclicBuffer<GraphSplineSegment>(capacity);
            _handles = new CyclicBuffer<Vector2>(3);
        }

        #endregion

        #region Add

        public bool Add(Vector2 node) {
            if (!_handles.Add(node)) return false;
            var buffer = _handles.GetBuffer();
            _segments.Add(new GraphSplineSegment(
                buffer[0],
                buffer[1],
                buffer[2]
            ));
            return true;
        }

        #endregion

        #region FillArray

        public void FillArray(GraphPoint[] destination) {
            var splinesBuffer = _segments.GetBuffer();

            for (var i = 0; i < destination.Length; i++) {
                var t = (float) i / (destination.Length - 1);
                destination[i] = Evaluate(splinesBuffer, t);
            }
        }

        #endregion

        #region Evaluate

        private GraphPoint Evaluate(GraphSplineSegment[] buffer, float t) {
            var tPerSpline = 1f / _segments.Size;
            var splineIndex = (int) (t / tPerSpline);
            if (splineIndex >= _segments.Size) splineIndex = _segments.Size - 1;
            var splineT = (t - tPerSpline * splineIndex) / tPerSpline;
            return buffer[splineIndex].Evaluate(splineT);
        }

        #endregion
    }
}
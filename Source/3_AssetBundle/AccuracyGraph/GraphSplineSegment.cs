using UnityEngine;

namespace BeatLeader {
    public class GraphSplineSegment {
        private readonly Vector2 _p00;
        private readonly Vector2 _p01;
        private readonly Vector2 _v00;
        private readonly Vector2 _v01;

        private Vector2 _p10;
        private Vector2 _p11;
        private Vector2 _v10;

        public GraphSplineSegment(
            Vector2 handleNodeA,
            Vector2 handleNodeB,
            Vector2 handleNodeC
        ) {
            _p00 = (handleNodeA + handleNodeB) / 2.0f;
            _p01 = handleNodeB;
            var p02 = (handleNodeB + handleNodeC) / 2.0f;
            _v00 = _p01 - _p00;
            _v01 = p02 - _p01;
        }

        public GraphPoint Evaluate(float t) {
            _p10 = _p00 + _v00 * t;
            _p11 = _p01 + _v01 * t;
            _v10 = _p11 - _p10;
            return new GraphPoint(
                _p10 + _v10 * t,
                _v10.normalized
            );
        }
    }
}
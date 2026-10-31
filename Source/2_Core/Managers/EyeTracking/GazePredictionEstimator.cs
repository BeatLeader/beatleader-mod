using UnityEngine;

namespace BeatLeader.EyeTracking {
    internal class GazePredictionEstimator {
        private const float DefaultSeconds = 0.047f;
        private const float MaxSeconds = 0.2f;
        private const float TimeConstant = 3f;
        private const float MaxOffset = 0.2f;
        private const float MaxVelocity = 10f;
        private const float MinVelocityVariance = 0.02f * 0.02f;

        public float Seconds { get; private set; } = DefaultSeconds;

        public bool IsMeasured { get; private set; }

        private float _weight;
        private Vector3 _velocitySum;
        private Vector3 _offsetSum;
        private float _velocitySquaredSum;
        private float _productSum;
        private float _lastTime = float.NegativeInfinity;

        public void Add(Vector3 gazeOffset, Vector3 headVelocity, float time) {
            var deltaTime = time - _lastTime;
            _lastTime = time;
            if (!(deltaTime > 0f && gazeOffset.magnitude < MaxOffset && headVelocity.magnitude < MaxVelocity)) return;

            var decay = Mathf.Exp(-Mathf.Min(deltaTime, 1f) / TimeConstant);
            _weight = _weight * decay + 1f;
            _velocitySum = _velocitySum * decay + headVelocity;
            _offsetSum = _offsetSum * decay + gazeOffset;
            _velocitySquaredSum = _velocitySquaredSum * decay + headVelocity.sqrMagnitude;
            _productSum = _productSum * decay + Vector3.Dot(headVelocity, gazeOffset);

            var meanVelocity = _velocitySum / _weight;
            var variance = _velocitySquaredSum / _weight - meanVelocity.sqrMagnitude;
            if (variance < MinVelocityVariance) return;

            var covariance = _productSum / _weight - Vector3.Dot(meanVelocity, _offsetSum / _weight);
            Seconds = Mathf.Clamp(covariance / variance, 0f, MaxSeconds);
            IsMeasured = true;
        }
    }
}

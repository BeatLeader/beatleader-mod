using UnityEngine;

namespace BeatLeader.EyeTracking {
    // SteamVR can report the angular velocity yawed against the poses, by an amount that changes between sessions
    internal class AngularVelocityYawEstimator {
        private const float TimeConstant = 10f;
        private const float MaxDeltaTime = 0.05f;
        private const float MaxAngularVelocity = 30f;
        private const float MinStrength = 0.05f * 0.05f;

        private float _weight;
        private float _crossSum;
        private float _dotSum;
        private float _cos = 1f;
        private float _sin;
        private Quaternion _lastRotation;
        private float _lastTime = float.NegativeInfinity;

        public void Add(Quaternion rotation, Vector3 angularVelocity, float time) {
            var deltaTime = time - _lastTime;
            var lastRotation = _lastRotation;
            _lastRotation = rotation;
            _lastTime = time;
            if (!(deltaTime > 0f && deltaTime <= MaxDeltaTime && angularVelocity.magnitude < MaxAngularVelocity)) return;

            var measured = MeasureAngularVelocity(lastRotation, rotation, deltaTime);
            var decay = Mathf.Exp(-deltaTime / TimeConstant);
            _weight = _weight * decay + 1f;
            _crossSum = _crossSum * decay + angularVelocity.z * measured.x - angularVelocity.x * measured.z;
            _dotSum = _dotSum * decay + angularVelocity.x * measured.x + angularVelocity.z * measured.z;

            var strength = Mathf.Sqrt(_crossSum * _crossSum + _dotSum * _dotSum) / _weight;
            if (strength < MinStrength) return;

            var yaw = Mathf.Atan2(_crossSum, _dotSum);
            _cos = Mathf.Cos(yaw);
            _sin = Mathf.Sin(yaw);
        }

        public Vector3 Apply(Vector3 angularVelocity) {
            return new Vector3(
                angularVelocity.x * _cos + angularVelocity.z * _sin,
                angularVelocity.y,
                angularVelocity.z * _cos - angularVelocity.x * _sin
            );
        }

        private static Vector3 MeasureAngularVelocity(Quaternion from, Quaternion to, float deltaTime) {
            var delta = to * new Quaternion(-from.x, -from.y, -from.z, from.w);
            if (delta.w < 0f) delta = new Quaternion(-delta.x, -delta.y, -delta.z, -delta.w);
            var axis = new Vector3(delta.x, delta.y, delta.z);
            var sine = axis.magnitude;
            return sine > 1e-7f ? axis * (2f * Mathf.Atan2(sine, delta.w) / (sine * deltaTime)) : Vector3.zero;
        }
    }
}

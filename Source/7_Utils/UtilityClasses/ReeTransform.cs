using UnityEngine;

namespace BeatLeader {
    public class ReeTransform {
        #region Constructor

        private Quaternion _rotation;
        private Quaternion _inverseRotation;

        public Vector3 Position;

        public Quaternion Rotation {
            get => _rotation;
            set {
                _rotation = value;
                _inverseRotation = Quaternion.Inverse(value);
            }
        }

        public ReeTransform(
            Vector3 position,
            Quaternion rotation
        ) {
            Position = position;
            Rotation = rotation;
        }

        #endregion

        #region LocalToWorld

        public Vector3 LocalToWorldPosition(Vector3 localPosition) {
            return Position + _rotation * localPosition;
        }

        public Vector3 LocalToWorldDirection(Vector3 localDirection) {
            return _rotation * localDirection;
        }

        public Quaternion LocalToWorldRotation(Quaternion localRotation) {
            return Rotation * localRotation;
        }

        #endregion

        #region WorldToLocal

        public Vector3 WorldToLocalPosition(Vector3 worldPosition) {
            return _inverseRotation * (worldPosition - Position);
        }

        public Vector3 WorldToLocalDirection(Vector3 worldDirection) {
            return _inverseRotation * worldDirection;
        }

        public Quaternion WorldToLocalRotation(Quaternion worldRotation) {
            return _inverseRotation * worldRotation;
        }

        #endregion
    }
}
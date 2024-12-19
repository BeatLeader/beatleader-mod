using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader {
    public struct ReeTransform {
        #region Constructor

        public Quaternion Rotation;

        public Vector3 Position;

        public static ReeTransform Identity => new ReeTransform(Vector3.zero, Quaternion.identity);
        public static ReeTransform PositiveInfinity => new ReeTransform(new Vector3(500, 500, 500), Quaternion.identity);

        public ReeTransform(in Vector3 position, in Quaternion rotation) {
            Position = position;
            Rotation = rotation;
        }

        public static ReeTransform FromTransform(Transform transform) {
            return new ReeTransform(transform.position, transform.rotation);
        }

        #endregion

        #region Properties

        [JsonIgnore]
        public readonly Quaternion InverseRotation => Quaternion.Inverse(Rotation);
        [JsonIgnore]
        public Vector3 Forward => LocalToWorldDirection(Vector3.forward);
        [JsonIgnore]
        public Vector3 Right => LocalToWorldDirection(Vector3.right);

        #endregion

        #region Child/Parent

        public static ReeTransform GetParentTransform(in ReeTransform childWorldTransform, in ReeTransform childLocalTransform) {
            var rotation = childWorldTransform.Rotation * childLocalTransform.InverseRotation;

            return new ReeTransform(
                childWorldTransform.Position - rotation * childLocalTransform.Position,
                rotation
            );
        }

        public static ReeTransform GetChildTransform(in ReeTransform parentWorldTransform, in ReeTransform childLocalTransform) {
            return new ReeTransform(
                parentWorldTransform.LocalToWorldPosition(childLocalTransform.Position),
                parentWorldTransform.LocalToWorldRotation(childLocalTransform.Rotation)
            );
        }

        #endregion

        #region LocalToWorld

        public readonly Vector3 LocalToWorldPosition(in Vector3 localPosition) {
            return Position + Rotation * localPosition;
        }

        public readonly Vector3 LocalToWorldDirection(in Vector3 localDirection) {
            return Rotation * localDirection;
        }

        public readonly Quaternion LocalToWorldRotation(in Quaternion localRotation) {
            return Rotation * localRotation;
        }

        #endregion

        #region WorldToLocal

        public readonly Vector3 WorldToLocalPosition(in Vector3 worldPosition) {
            return InverseRotation * (worldPosition - Position);
        }

        public readonly Vector3 WorldToLocalDirection(in Vector3 worldDirection) {
            return InverseRotation * worldDirection;
        }

        public readonly Quaternion WorldToLocalRotation(in Quaternion worldRotation) {
            return InverseRotation * worldRotation;
        }

        #endregion

        #region Equals

        public bool Equals(ReeTransform other) {
            return Rotation.Equals(other.Rotation) && Position.Equals(other.Position);
        }

        public override bool Equals(object obj) {
            return obj is ReeTransform other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return Rotation.GetHashCode() * 397 ^ Position.GetHashCode();
            }
        }

        #endregion
    }
}
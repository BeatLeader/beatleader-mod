using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using System;

namespace BeatLeader.Models {
    internal struct SerializableVector2 {
        public SerializableVector2(UVector2 unityVector) {
            x = unityVector.x;
            y = unityVector.y;
        }
        public SerializableVector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public float this[int idx] {
            get {
                return idx switch {
                    0 => x,
                    1 => y,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }

        public float x;
        public float y;

        public static implicit operator UVector3(SerializableVector2 vector) => new(vector.x, vector.y);
        public static implicit operator UVector2(SerializableVector2 vector) => new(vector.x, vector.y);
        public static implicit operator SerializableVector2(UVector2 vector) => new(vector);
    }
}

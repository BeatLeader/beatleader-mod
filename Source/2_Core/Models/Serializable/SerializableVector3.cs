using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeatLeader.Models {
    [StructLayout(LayoutKind.Auto)]
    public struct SerializableVector3 {
        public SerializableVector3(Vector3 unityVector) {
            x = unityVector.x;
            y = unityVector.y;
            z = unityVector.z;
        }
        public SerializableVector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float this[int idx] {
            get {
                return idx switch {
                    0 => x,
                    1 => y,
                    2 => z,
                    _ => throw new IndexOutOfRangeException()
                };
            }
        }

        public float x;
        public float y;
        public float z;

        public static implicit operator Vector3(SerializableVector3 vector) => new(vector.x, vector.y, vector.z);
        public static implicit operator Vector2(SerializableVector3 vector) => new(vector.x, vector.y);
        public static implicit operator SerializableVector3(Vector3 vector) => new(vector);
    }
}

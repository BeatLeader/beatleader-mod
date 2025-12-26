using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeatLeader.Models {
    [StructLayout(LayoutKind.Auto)]
    public struct SerializableVector2 {
        public SerializableVector2(Vector2 unityVector) {
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

        public static implicit operator Vector3(SerializableVector2 vector) => new(vector.x, vector.y);
        public static implicit operator Vector2(SerializableVector2 vector) => new(vector.x, vector.y);
        public static implicit operator SerializableVector2(Vector2 vector) => new(vector);
    }
}

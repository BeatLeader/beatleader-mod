using UnityEngine;

namespace BeatLeader.Models {
    public struct SerializableQuaternion {
        public SerializableQuaternion(Quaternion unityQuaternion) {
            x = unityQuaternion.x;
            y = unityQuaternion.y;
            z = unityQuaternion.z;
            w = unityQuaternion.w;
        }

        public float x;
        public float y;
        public float z;
        public float w;

        public static implicit operator Quaternion(SerializableQuaternion quaternion) => new(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        public static implicit operator SerializableQuaternion(Quaternion quaternion) => new(quaternion);
    }
}

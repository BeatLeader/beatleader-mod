using System.Runtime.InteropServices;
using UnityEngine;

namespace BeatLeader.Models {
    [StructLayout(LayoutKind.Auto)]
    public struct SerializablePose {
        public SerializablePose(SerializableVector3 position, SerializableQuaternion rotation) {
            this.position = position; 
            this.rotation = rotation;
        }

        public SerializableVector3 position;
        public SerializableQuaternion rotation;

        public static implicit operator Pose(SerializablePose pose) => new(pose.position, pose.rotation);
        public static implicit operator SerializablePose(Pose pose) => new(pose.position, pose.rotation);
    }
}

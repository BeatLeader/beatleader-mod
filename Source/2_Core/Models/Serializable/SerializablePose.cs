using UnityEngine;

namespace BeatLeader.Models {
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

    public struct FullSerializablePose {
        public FullSerializablePose(SerializableVector3 position, SerializableVector3 scale, SerializableQuaternion rotation) {
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
        }

        public SerializableVector3 position;
        public SerializableVector3 scale;
        public SerializableQuaternion rotation;

        public static implicit operator Pose(FullSerializablePose pose) => new(pose.position, pose.rotation);
        public static implicit operator FullSerializablePose(Pose pose) => new(pose.position, new(1f, 1f, 1f), pose.rotation);
    }
}

using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Replayer {
    public sealed class ManualCameraView : CameraView {
        public override string Name { get; init; } = "CustomView";

        public SerializableVector3 Position { get; set; }
        public float Rotation { get; set; }

        public void Reset() {
            Position = Vector3.zero;
            Rotation = 0;
        }

        public override Pose ProcessPose(Pose headPose) {
            return new(Position, Quaternion.Euler(0, Rotation, 0));
        }
    }
}
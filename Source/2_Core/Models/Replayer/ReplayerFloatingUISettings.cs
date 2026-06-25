using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerFloatingUISettings {
        public SerializablePose InitialPose { get; set; }
        public SerializablePose Pose { get; set; }
        public bool Pinned { get; set; }
        public bool SnapEnabled { get; set; }
        public float CurvatureRadius { get; set; }
        public bool CurvatureEnabled { get; set; }
        public bool AttachToHand { get; set; }
        public bool AttachToRightHand { get; set; }
        public SerializablePose HandOffset { get; set; } = new(
            new SerializableVector3(0f, 0.12f, 0.24f),
            Quaternion.Euler(65f, 0f, 0f)
        );
        public float HandScale { get; set; } = 0.3f;
    }
}
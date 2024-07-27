using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerFloatingUISettings {
        public SerializablePose InitialPose { get; set; }
        public SerializablePose Pose { get; set; }
        public bool Pinned { get; set; }
        public bool SnapEnabled { get; set; }
        public float CurvatureRadius { get; set; }
        public bool CurvatureEnabled { get; set; }
    }
}
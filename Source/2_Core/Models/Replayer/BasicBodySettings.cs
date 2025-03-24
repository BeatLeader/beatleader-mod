using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BasicBodySettings {
        public bool HeadEnabled { get; set; }
        public bool TorsoEnabled { get; set; }
        public bool LeftHandEnabled { get; set; }
        public bool RightHandEnabled { get; set; }

        public bool LeftSaberEnabled { get; set; }
        public bool RightSaberEnabled { get; set; }
    }
}
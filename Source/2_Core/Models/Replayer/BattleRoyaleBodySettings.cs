using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BattleRoyaleBodySettings : BasicBodySettings {
        public bool TrailEnabled { get; set; }
        public float TrailLength { get; set; }
        public float TrailOpacity { get; set; }
    }
}
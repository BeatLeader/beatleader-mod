using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class BattleRoyaleBodySettings : BasicBodySettings {
        public float TrailLength { get; set; }
        public float TrailOpacity { get; set; }
    }
}
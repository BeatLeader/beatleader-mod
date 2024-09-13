using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public readonly struct BeatmapLevelWithKey {
        public BeatmapLevelWithKey(BeatmapLevel level, BeatmapKey key) {
            Level = level;
            Key = key;
            HasValue = true;
        }
        
        public readonly BeatmapLevel Level;
        public readonly BeatmapKey Key;
        public readonly bool HasValue;
    }
}
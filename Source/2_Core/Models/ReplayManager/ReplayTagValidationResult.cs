using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public readonly struct ReplayTagValidationResult {
        public ReplayTagValidationResult(bool hasValidSignature, bool exclusive) {
            HasValidSignature = hasValidSignature;
            Exclusive = exclusive;
        }
        
        public readonly bool HasValidSignature;
        public readonly bool Exclusive;
        
        public bool Ok => HasValidSignature && Exclusive;
    }
}
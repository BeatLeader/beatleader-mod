namespace BeatLeader.Models {
    public record ReplayTagValidationResult(
        bool HasValidSignature,
        bool Exclusive
    ) {
        public bool Ok => HasValidSignature && Exclusive;
    }
}
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public enum ReplaySavingError {
        None,
        AlreadyExists,
        WritingFailed,
        ValidationFailed
    }
}
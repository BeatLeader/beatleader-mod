using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public readonly struct ReplaySavingResult {
        public ReplaySavingResult(IReplayHeader header) {
            Header = header;
            Error = ReplaySavingError.None;
        }

        public ReplaySavingResult(ReplaySavingError error) {
            Header = null;
            Error = error;
        }

        public readonly IReplayHeader? Header;
        public readonly ReplaySavingError Error;

        public bool Ok => Error is ReplaySavingError.None;
    }
}
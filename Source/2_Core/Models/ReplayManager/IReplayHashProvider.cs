namespace BeatLeader.Models {
    /// <summary>
    /// An interface that is applied to objects that represent replays,
    /// like Score or ReplayInfo. Provides basic properties for hash calculation.
    /// Not intended for external usage.
    /// </summary>
    public interface IReplayHashProvider {
        string PlayerID { get; }
        long Timestamp { get; }
    }

    public static class ReplayHashExtensions {
        public static int CalculateReplayHash(this IReplayHashProvider provider) {
            unchecked {
                var hash = 17; // seed
                hash = hash * 31 + provider.Timestamp.GetHashCode();
                hash = hash * 31 + provider.PlayerID.GetHashCode();

                return hash;
            }
        }
    }
}
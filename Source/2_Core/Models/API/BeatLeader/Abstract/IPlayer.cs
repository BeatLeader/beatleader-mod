using System.Threading;
using System.Threading.Tasks;
using BeatSaber.BeatAvatarSDK;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IPlayer {
        string Id { get; }
        string Name { get; }
        string? AvatarUrl { get; }
        int Rank { get; }
        int CountryRank { get; }
        int Level { get; }
        int Experience { get; }
        int Prestige { get; }
        string Country { get; }
        float PerformancePoints { get; }
        IPlayerProfileSettings? ProfileSettings { get; }

        /// <summary>
        /// Fetches beat avatar from the server.
        /// </summary>
        /// <param name="bypassCache">Determines if the cached value should be omitted.</param>
        /// <returns>An avatar data.</returns>
        Task<AvatarData> GetBeatAvatarAsync(bool bypassCache, CancellationToken token);
    }
}
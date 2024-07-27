using System.Threading.Tasks;

namespace BeatLeader.Models {
    public interface IPlayer {
        string Id { get; }
        string Name { get; }
        string? AvatarUrl { get; }
        int Rank { get; }
        int CountryRank { get; }
        string Country { get; }
        float PerformancePoints { get; }
        IPlayerProfileSettings? ProfileSettings { get; }

        Task<AvatarSettings> GetAvatarAsync(bool bypassCache = false);
    }
}
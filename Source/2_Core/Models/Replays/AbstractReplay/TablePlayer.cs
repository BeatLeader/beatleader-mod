using UnityEngine;

namespace BeatLeader.Models.AbstractReplay {
    public record TablePlayer(
        string Id,
        string Name,
        string? AvatarUrl,
        int Rank,
        int CountryRank,
        string Country,
        float PerformancePoints,
        IPlayerProfileSettings? ProfileSettings,
        Color AccentColor
    ) : ITablePlayer {
        public static ITablePlayer CreateFromPlayer(IPlayer player, Color accentColor) {
            return new TablePlayer(
                player.Id, 
                player.Name,
                player.AvatarUrl,
                player.Rank,
                player.CountryRank,
                player.Country,
                player.PerformancePoints, 
                player.ProfileSettings,
                accentColor
            );
        }
    }
}
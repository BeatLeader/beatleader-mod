using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    internal static class ReplayManagerCache {
        private static readonly Dictionary<string, IPlayer> cachedPlayers = new();

        public static void AddPlayer(IPlayer player) {
            cachedPlayers[player.Id] = player;
        }

        public static IPlayer? GetPlayer(string id) {
            cachedPlayers.TryGetValue(id, out var player);
            return player;
        }

    }
}
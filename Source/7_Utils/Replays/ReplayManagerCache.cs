using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    internal static class ReplayManagerCache {
        private static readonly Dictionary<string, Player> cachedPlayers = new();

        public static void AddPlayer(Player player) {
            cachedPlayers[player.id] = player;
        }

        public static Player? GetPlayer(string id) {
            cachedPlayers.TryGetValue(id, out var player);
            return player;
        }

    }
}
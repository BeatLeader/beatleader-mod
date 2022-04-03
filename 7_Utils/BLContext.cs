using System;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    // TODO: Replace by smth not static to store app context
    internal static class BLContext {
        public static Player? profile;
        public static string steamAuthToken;

        public static bool IsCurrentPlayer(this Player player) {
            return string.Equals(player.id, profile?.id, StringComparison.Ordinal);
        }
        
        public static Score TestScore => new Score {
            accuracy = 0.9641f,
            baseScore = 69420,
            modifiedScore = 69420,
            modifiers = "LOL,KEK",
            pp = 322.0f,
            rank = 19,
            player = new Player {
                id = "0",
                rank = 1337,
                name = "Chuck Norris",
                avatar = "https://media2.giphy.com/media/BIuuwHRNKs15C/200.gif",
                country = "kz",
                countryRank = 1,
                pp = 399.0f,
                role = "Meme"
            }
        };
    }
}
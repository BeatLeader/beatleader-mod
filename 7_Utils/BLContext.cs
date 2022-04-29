using System;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    // TODO: Replace by smth not static to store app context
    internal static class BLContext {

        public static Player? profile;

        public static bool NoPlayerData {
            get {
                return profile == null;
            }
        }

        public static bool IsCurrentPlayer(this Player player) {
            return string.Equals(player.id, profile?.id, StringComparison.Ordinal);
        }
    }
}
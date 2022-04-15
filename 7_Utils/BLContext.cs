using System;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    // TODO: Replace by smth not static to store app context
    internal static class BLContext {
        public static Player? profile;

        public static bool IsCurrentPlayer(this Player player) {
            return string.Equals(player.id, profile?.id, StringComparison.Ordinal);
        }

        public static ScoresContext DefaultScoresContext => ScoresContext.Modifiers;
        public static ScoresScope DefaultScoresScope => ScoresScope.Global;
    }
}
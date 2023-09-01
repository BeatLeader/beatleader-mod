namespace BeatLeader.Models {
    internal enum ScoresContext {
        Modifiers,
        Standard,
        Nopause,
        Golf
    }

    internal static class ScoresContextExtenstions {
        internal static string Name(this ScoresContext context) {
            switch (context) {
                case ScoresContext.Modifiers: return "General";
                case ScoresContext.Standard: return "No Mods";
                case ScoresContext.Nopause: return "No Pauses";
                case ScoresContext.Golf: return "Golf";
            }

            return "General";
        }

        internal static LeaderboardContexts Enum(this ScoresContext context) {
            switch (context) {
                case ScoresContext.Modifiers: return LeaderboardContexts.General;
                case ScoresContext.Standard: return LeaderboardContexts.NoMods;
                case ScoresContext.Nopause: return LeaderboardContexts.NoPause;
                case ScoresContext.Golf: return LeaderboardContexts.Golf;
            }

            return LeaderboardContexts.General;
        }

        internal static ScoresContext FromName(string name) {
            switch (name) {
                case "General": return ScoresContext.Modifiers;
                case "No Mods": return ScoresContext.Standard;
                case "No Pauses": return ScoresContext.Nopause;
                case "Golf": return ScoresContext.Golf;
            }

            return ScoresContext.Modifiers;
        }
    }
}
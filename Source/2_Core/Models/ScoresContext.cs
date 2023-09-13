using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    internal enum ScoresContext {
        Modifiers,
        Standard,
        Nopause,
        Golf
    }

    internal static class ScoresContextExtenstions {
        private static readonly ScoresContext[] availableContexts = {
            ScoresContext.Modifiers,
            ScoresContext.Standard,
            ScoresContext.Nopause,
            ScoresContext.Golf
        };

        internal static IEnumerable<ScoresContext> GetAvailableContexts() {
            return availableContexts;
        }

        internal static Sprite Icon(this ScoresContext context) {
            //TODO: Make Icons
            return context switch {
                ScoresContext.Modifiers => BundleLoader.ModifiersIcon,
                ScoresContext.Standard => BundleLoader.DefaultAvatar,
                ScoresContext.Nopause => BundleLoader.PauseIcon,
                ScoresContext.Golf => BundleLoader.LocationIcon,
                _ => BundleLoader.ModifiersIcon
            };
        }

        internal static string Name(this ScoresContext context) {
            return context switch {
                ScoresContext.Modifiers => "General",
                ScoresContext.Standard => "No Mods",
                ScoresContext.Nopause => "No Pauses",
                ScoresContext.Golf => "Golf",
                _ => "General"
            };
        }

        internal static LeaderboardContexts Enum(this ScoresContext context) {
            return context switch {
                ScoresContext.Modifiers => LeaderboardContexts.General,
                ScoresContext.Standard => LeaderboardContexts.NoMods,
                ScoresContext.Nopause => LeaderboardContexts.NoPause,
                ScoresContext.Golf => LeaderboardContexts.Golf,
                _ => LeaderboardContexts.General
            };
        }

        internal static ScoresContext FromName(string name) {
            return name switch {
                "General" => ScoresContext.Modifiers,
                "No Mods" => ScoresContext.Standard,
                "No Pauses" => ScoresContext.Nopause,
                "Golf" => ScoresContext.Golf,
                _ => ScoresContext.Modifiers
            };
        }
    }
}
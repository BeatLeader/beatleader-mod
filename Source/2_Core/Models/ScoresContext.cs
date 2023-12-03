using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    internal enum ScoresContext {
        Modifiers,
        Standard,
        Nopause,
        Golf,
        SCPM
    }

    internal static class ScoresContextExtenstions {
        private static readonly ScoresContext[] availableContexts = {
            ScoresContext.Modifiers,
            ScoresContext.Standard,
            ScoresContext.Nopause,
            ScoresContext.Golf,
            ScoresContext.SCPM
        };

        internal static IEnumerable<ScoresContext> GetAvailableContexts() {
            return availableContexts;
        }

        internal static Sprite Icon(this ScoresContext context) {
            //TODO: Make Icons
            return context switch {
                ScoresContext.Modifiers => BundleLoader.GeneralContextIcon,
                ScoresContext.Standard => BundleLoader.NoModifiersIcon,
                ScoresContext.Nopause => BundleLoader.NoPauseIcon,
                ScoresContext.Golf => BundleLoader.GolfIcon,
                ScoresContext.SCPM => BundleLoader.SCPMIcon,
                _ => BundleLoader.GeneralContextIcon
            };
        }

        internal static string LocalizedName(this ScoresContext context) {
            return context switch {
                ScoresContext.Modifiers => "<bll>ls-general-context</bll>",
                ScoresContext.Standard => "<bll>ls-no-mods-context</bll>",
                ScoresContext.Nopause => "<bll>ls-no-pauses-context</bll>",
                ScoresContext.Golf => "<bll>ls-golf-context</bll>",
                ScoresContext.SCPM => "<bll>ls-scpm-context</bll>",
                _ => "<bll>ls-general-context</bll>"
            };
        }

        internal static string Description(this ScoresContext context) {
            return context switch {
                ScoresContext.Modifiers => "Everything allowed",
                ScoresContext.Standard => "No modifiers, except 1 Live and 4 Lives",
                ScoresContext.Nopause => "Pauses are forbidden",
                ScoresContext.Golf => "The worse you play - the higher score",
                ScoresContext.SCPM => "Only scores with Small Cubes and Pro Mode",
                _ => "General"
            };
        }

        internal static LeaderboardContexts Enum(this ScoresContext context) {
            return context switch {
                ScoresContext.Modifiers => LeaderboardContexts.General,
                ScoresContext.Standard => LeaderboardContexts.NoMods,
                ScoresContext.Nopause => LeaderboardContexts.NoPause,
                ScoresContext.Golf => LeaderboardContexts.Golf,
                ScoresContext.SCPM => LeaderboardContexts.SCPM,
                _ => LeaderboardContexts.General
            };
        }

        internal static ScoresContext FromName(string name) {
            return name switch {
                "General" => ScoresContext.Modifiers,
                "No Mods" => ScoresContext.Standard,
                "No Pauses" => ScoresContext.Nopause,
                "Golf" => ScoresContext.Golf,
                "SCPM" => ScoresContext.SCPM,
                _ => ScoresContext.Modifiers
            };
        }
    }
}
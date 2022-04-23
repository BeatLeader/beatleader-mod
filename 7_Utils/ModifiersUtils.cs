using System.Collections.Generic;

namespace BeatLeader.Utils {
    internal class ModifiersUtils : PersistentSingleton<ModifiersUtils> {

        private Dictionary<string, float> _modifiers;

        internal Dictionary<string, float> Modifiers {
            get => _modifiers;
            set => _modifiers = value;
        }

        internal bool HasModifiers {
            get {
                return Modifiers?.Count > 0;
            }
        }

        private static readonly Dictionary<string, string> _modifierNameToCodeMapping = new() {
            { "MODIFIER_NO_FAIL_ON_0_ENERGY", "NF" },
            { "MODIFIER_ONE_LIFE"           , "IF" },
            { "MODIFIER_FOUR_LIVES"         , "BE" },
            { "MODIFIER_NO_BOMBS"           , "NB" },
            { "MODIFIER_NO_OBSTACLES"       , "NO" },
            { "MODIFIER_NO_ARROWS"          , "NA" },
            { "MODIFIER_GHOST_NOTES"        , "GN" },
            { "MODIFIER_DISAPPEARING_ARROWS", "DA" },
            { "MODIFIER_SMALL_CUBES"        , "SC" },
            { "MODIFIER_PRO_MODE"           , "PM" },
            { "MODIFIER_STRICT_ANGLES"      , "SA" },
            { "MODIFIER_ZEN_MODE"           , "ZM" },
            { "MODIFIER_SLOWER_SONG"        , "SS" },
            { "MODIFIER_FASTER_SONG"        , "FS" },
            { "MODIFIER_SUPER_FAST_SONG"    , "SF" }
        };

        internal static string ToNameCode(string localizationKey) {
            return _modifierNameToCodeMapping[localizationKey];
        }

        internal static string GetRankForMultiplier(float modifier) {
            if (modifier > 0.9f) {
                return "SS";
            }
            if (modifier > 0.8f) {
                return "S";
            }
            if (modifier > 0.65f) {
                return "A";
            }
            if (modifier > 0.5f) {
                return "B";
            }
            if (modifier > 0.35f) {
                return "C";
            }
            if (modifier > 0.2f) {
                return "D";
            }
            return "E";
        }
    }
}

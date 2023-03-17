using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    internal static class ModifiersUtils {
        
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

        public static float GetMultiplier(this ModifiersMap modifiersMap, string modifierCode) {
            return modifierCode switch {
                "DA" => modifiersMap.da,
                "FS" => modifiersMap.fs,
                "SS" => modifiersMap.ss,
                "SF" => modifiersMap.sf,
                "GN" => modifiersMap.gn,
                "NA" => modifiersMap.na,
                "NB" => modifiersMap.nb,
                "NF" => modifiersMap.nf,
                "NO" => modifiersMap.no,
                "PM" => modifiersMap.pm,
                "SC" => modifiersMap.sc,
                _ => 0.0f
            };
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

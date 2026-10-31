using System;
using System.Collections.Generic;
using BeatLeader.Models;
using ModifiersCore;

namespace BeatLeader.Utils {
    internal static class ModifiersUtils {
        // BFS and BSF have their own ratings, but take the FS and SF multipliers
        private static readonly Dictionary<string, string> multiplierIds = new() {
            { "BFS", "FS" },
            { "BSF", "SF" }
        };

        internal static float GetMultiplier(this Dictionary<string, float>? modifiersMap, IModifier modifier) {
            var hasBaseId = multiplierIds.TryGetValue(modifier.Id, out var id);
            if (!hasBaseId) id = modifier.Id;

            if (modifiersMap != null && modifiersMap.TryGetValue(id.ToLower(), out var value)) {
                return value;
            }
            // Other custom modifiers fall back to their own value, base-game ones like Zen Mode's -1000% don't apply
            return modifier is ICustomModifier && !hasBaseId ? modifier.Multiplier : 0f;
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
using System;
using System.Collections.Generic;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    internal static class ModifiersUtils {
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
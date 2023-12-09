using UnityEngine;
using Random = System.Random;

namespace BeatLeader.Utils {
    public static class ColorUtils {
        private static readonly Random random = new();

        public static Color RandomColor(int precision = 1000, int startFrom = 300) {
            var r = Random01(precision, startFrom);
            var g = Random01(precision, startFrom);
            var b = Random01(precision, startFrom);
            return new(r, g, b);

            static float Random01(int precision, int startFrom) {
                return random.Next(startFrom, precision) / (float)precision;
            }
        }
    }
}
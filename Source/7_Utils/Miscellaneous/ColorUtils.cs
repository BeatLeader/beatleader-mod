using UnityEngine;
using Random = System.Random;

namespace BeatLeader.Utils {
    public static class ColorUtils {
        private static readonly Random random = new();

        public static Color RandomColor(int precision = 1000, int startFrom = 300, Random? rand = null) {
            rand ??= random;
            var r = Random01(precision, startFrom, rand);
            var g = Random01(precision, startFrom, rand);
            var b = Random01(precision, startFrom, rand);
            return new(r, g, b);

            static float Random01(int precision, int startFrom, Random rand) {
                return rand.Next(startFrom, precision) / (float)precision;
            }
        }
    }
}
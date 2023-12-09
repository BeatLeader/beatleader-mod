using UnityEngine;
using Random = System.Random;

namespace BeatLeader.Utils {
    public static class ColorUtils {
        public static Color RandomColor(int precision = 1000) {
            var random = new Random();
            var r = random.Next(0, precision) / (float)precision;
            var g = random.Next(0, precision) / (float)precision;
            var b = random.Next(0, precision) / (float)precision;
            return new(r, g, b);
        }
    }
}
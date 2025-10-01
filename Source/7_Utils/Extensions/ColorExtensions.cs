using UnityEngine;

namespace BeatLeader.Utils {
    internal static class ColorExtensions {
        public static Color SetAlpha(this ref Color color, float alpha) {
            color.a = alpha;
            return color;
        }
    }
}

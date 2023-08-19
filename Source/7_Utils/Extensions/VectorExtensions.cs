using UnityEngine;

namespace BeatLeader.Utils {
    internal static class VectorExtensions {
        public static Vector2 Invert(this Vector2 vector) {
            for (int i = 0; i < 2; i++) {
                vector[i] = MathUtils.Map(vector[i], 0, 1, 1, 0);
            }
            return vector;
        }
    }
}

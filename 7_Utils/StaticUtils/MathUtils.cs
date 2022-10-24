namespace BeatLeader.Utils {
   internal static class MathUtils {
        public static float Map(float val, float inMin, float inMax, float outMin, float outMax) {
            return (val - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
        public static float GetClosestCoordinate(float pos, float inc) {
            var point1 = pos - (pos % inc);
            var point2 = point1 + inc;

            return pos > point2 - (inc / 2) ? point2 : point1;
        }
    }
}

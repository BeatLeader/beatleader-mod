using System;
using UnityEngine;

namespace BeatLeader.Utils {
    internal static class MathUtils {
        public static float Map(float val, float inMin, float inMax, float outMin, float outMax) {
            return (val - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static Vector2 ToAbsPos(Vector2 relative, Vector2 zone) {
            zone /= 2;
            for (int i = 0; i < 2; i++) {
                relative[i] = Map(relative[i], 0, 1, -zone[i], zone[i]);
            }
            return relative;
        }

        public static Vector2 ToRelPos(Vector2 abs, Vector2 zone) {
            zone /= 2;
            for (int i = 0; i < 2; i++) {
                abs[i] = Map(abs[i], -zone[i], zone[i], 0, 1);
            }
            return abs;
        }

        public static float GetClosestCoordinate(float pos, float inc) {
            var point1 = pos - (pos % inc);
            var point2 = point1 + inc;

            return pos > point2 - (inc / 2) ? point2 : point1;
        }

        public static float IncorporateBool(this float val, bool flag) {
            int i = BitConverter.ToInt32(BitConverter.GetBytes(val), 0);
            if (flag) {
                i |= 1;
            } else {
                i &= ~1;
            }
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }
    }
}
using System;
using UnityEngine;

namespace BeatLeader.Utils {
    internal static class MathUtils {
        public static float RoundStepped(float value, float step, float startValue = 0) {
            if (step is 0) return value;
            var relativeValue = value - startValue;
            return startValue + Mathf.Round(relativeValue / step) * step;
        }
        
        public static float Map(float val, float inMin, float inMax, float outMin, float outMax) {
            return (val - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        public static float GetClosestCoordinate(float pos, float inc) {
            var point1 = pos - (pos % inc);
            var point2 = point1 + inc;
            return pos > point2 - (inc / 2) ? point2 : point1;
        }

        public static Vector2 Clamp(this Vector2 vector, Vector2 minVector, Vector2 maxVector) {
            for (var i = 0; i < 2; i++) {
                vector[i] = Mathf.Clamp(vector[i], minVector[i], maxVector[i]);
            }
            return vector;
        }

        public static float IncorporateBool(this float val, bool flag) {
            var i = BitConverter.ToInt32(BitConverter.GetBytes(val), 0);
            if (flag) i |= 1;
            else i &= ~1;
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }
    }
}
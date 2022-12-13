using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class MathUtils
    {
        public static float Map(float val, float inMin, float inMax, float outMin, float outMax)
        {
            return (val - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
        public static float GetClosestCoordinate(float pos, float inc)
        {
            var point1 = pos - (pos % inc);
            var point2 = point1 + inc;

            return pos > point2 - (inc / 2) ? point2 : point1;
        }
        public static float ToPositive(this float val)
        {
            return val < 0 ? val * -1 : val;
        }
        public static float ToNegative(this float val)
        {
            return val > 0 ? val * -1 : val;
        }

        public static float IncorporateBool(this float val, bool flag)
        {
            int i = BitConverter.ToInt32(BitConverter.GetBytes(val), 0);
            if (flag)
            {
                i |= 1;
            }
            else
            {
                i &= ~1;
            }
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }
    }
}

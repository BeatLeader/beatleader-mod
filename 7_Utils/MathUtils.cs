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
        public static float Difference(float first, float second)
        {
            return Mathf.Max(first, second).ToPositive() - Mathf.Min(first, second).ToPositive();
        }
        public static float ToPositive(this float val)
        {
            return val < 0 ? val * -1 : val;
        }
        public static float ToNegative(this float val)
        {
            return val > 0 ? val * -1 : val;
        }
    }
}

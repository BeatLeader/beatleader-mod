using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class Vector3Extension
    {
        public static float GetDimension(this Vector3 vec, int dimension)
        {
            return dimension switch
            {
                0 => vec.x,
                1 => vec.y,
                2 => vec.z,
                _ => -1
            };
        }
        public static void SetDimension(this ref Vector3 vec, int dimension, float val)
        {
            switch (dimension)
            {
                case 0:
                    vec.x = val;
                    break;
                case 1:
                    vec.y = val;
                    break;
                case 2:
                    vec.z = val;
                    break;
            }
        }
        public static bool Compare(this Vector3 l, Vector3 r, float acc)
        {
            bool flag = Mathf.Abs(l.x - r.x) <= acc;
            bool flag2 = Mathf.Abs(l.y - r.y) <= acc;
            bool flag3 = Mathf.Abs(l.z - r.z) <= acc;
            return flag && flag2 && flag3;
        }
    }
}

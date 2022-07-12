using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class TransformExtension
    {
        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            for(int idx = 0; idx < transform.childCount; idx++)
            {
                children.Add(transform.GetChild(idx));
            }
            return children;
        }
    }
}

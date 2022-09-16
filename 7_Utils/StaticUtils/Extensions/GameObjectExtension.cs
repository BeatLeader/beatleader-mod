using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class GameObjectExtension
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (!go.TryGetComponent(out T component))
                component = go.AddComponent<T>();
            return component;
        }
        public static bool TryDestroy(this GameObject go)
        {
            if (go == null) return false;
            GameObject.Destroy(go);
            return true;
        }
        public static bool TryDestroy<T>(this T go) where T : Component
        {
            if (go == null) return false;
            GameObject.Destroy(go);
            return true;
        }
    }
}

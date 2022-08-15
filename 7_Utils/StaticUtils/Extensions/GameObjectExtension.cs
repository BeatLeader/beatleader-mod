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
        public static T GetComponentInParentHierarchy<T>(this GameObject go) where T : Component
        {
            var parent = go.transform.parent.gameObject;
            var component = parent.GetComponent<T>();
            return component != null ? component : parent?.GetComponentInParentHierarchy<T>() ;
        }
        public static bool TryDestroy<T>(this T go) where T : Component
        {
            if (go == null) return false;
            GameObject.Destroy(go);
            return true;
        }
    }
}

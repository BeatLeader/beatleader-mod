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
        public static bool TryDestroy(this Object obj)
        {
            if (obj == null) return false;
            Object.Destroy(obj);
            return true;
        }
    }
}

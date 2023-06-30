using System;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour {
        static MonoSingleton() {
            MonoSingletonDummy.AssignFactory(() => {
                var comp = new GameObject(typeof(T).Name).AddComponent<T>();
                DontDestroyOnLoad(comp);
                return comp;
            });
        }

        [UsedImplicitly]
        private class MonoSingletonDummy : Singleton<T> {
            public new static void AssignFactory(Func<T> factory) {
                Singleton<T>.AssignFactory(factory);
            }
        }

        public static T Instance => MonoSingletonDummy.Instance;
    }
}
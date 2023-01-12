using UnityEngine;
using Zenject;

namespace BeatLeader.Utils
{
    public static class ZenjectExtension
    {
        public static void InjectComponentsInChildren(this DiContainer container, GameObject gameObject) {
            foreach (var item in gameObject.GetComponentsInChildren<Component>()) {
                container.Inject(item);
            }
        }

        public static T InstantiateComponentOnNewGameObjectSelf<T>(this DiContainer container) where T : Component
        {
            var component = container.InstantiateComponentOnNewGameObject<T>();
            container.Bind<T>().FromInstance(component).AsSingle().NonLazy();
            return component;
        }
    }
}

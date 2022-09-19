using UnityEngine;
using Zenject;

namespace BeatLeader.Utils
{
    public static class ZenjectExtension
    {
        public static T InstantiateComponentOnNewGameObjectSelf<T>(this DiContainer container) where T : Component
        {
            var component = container.InstantiateComponentOnNewGameObject<T>();
            container.Bind<T>().FromInstance(component).AsSingle().NonLazy();
            return component;
        }
    }
}

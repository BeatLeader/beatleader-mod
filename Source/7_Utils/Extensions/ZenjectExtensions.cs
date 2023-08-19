using System.Reflection;
using UnityEngine;
using Zenject;

namespace BeatLeader.Utils {
    public static class ZenjectExtensions {
        #region GetContainer

        private static readonly PropertyInfo containerPropertyInfo = typeof(MonoInstallerBase)
            .GetProperty("Container", ReflectionUtils.DefaultFlags)!;

        public static DiContainer GetContainer(this MonoInstallerBase monoInstallerBase) {
            return (DiContainer)containerPropertyInfo.GetValue(monoInstallerBase);
        }

        #endregion

        #region Injection

        public static void InjectComponentsInChildren(this DiContainer container, GameObject gameObject) {
            foreach (var item in gameObject.GetComponentsInChildren<Component>()) {
                container.Inject(item);
            }
        }

        #endregion

        #region Installation

        public static T InstantiateComponentOnNewGameObjectSelf<T>(this DiContainer container) where T : Component {
            var component = container.InstantiateComponentOnNewGameObject<T>();
            container.Bind<T>().FromInstance(component).AsSingle().NonLazy();
            return component;
        }

        #endregion
    }
}
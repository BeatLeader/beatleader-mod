using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal abstract class ReeUIComponentV2WithContainer : ReeUIComponentV2 {
        protected enum InjectionTimings {
            None,
            Instantiate,
            Initialize
        }

        protected virtual InjectionTimings InjectionTiming { get; }
        protected DiContainer Container { get; private set; }

        public static T InstantiateInContainer<T>(DiContainer container, Transform parent, bool parseImmediately = true) where T : ReeUIComponentV2WithContainer {
            var component = container.InstantiateComponentOnNewGameObjectSelf<T>();
            component.Container = container;
            component.OnInstantiate();
            component.Setup(parent, parseImmediately);
            return component;
        }
    }
}

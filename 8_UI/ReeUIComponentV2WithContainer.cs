using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal abstract class ReeUIComponentV2WithContainer : ReeUIComponentV2 {
        public static T InstantiateInContainer<T>(DiContainer container, Transform parent, bool parseImmediately = true) where T : ReeUIComponentV2WithContainer {
            var component = container.InstantiateComponentOnNewGameObject<T>();
            component.OnInstantiate();
            component.Setup(parent, parseImmediately);
            return component;
        }
    }
}

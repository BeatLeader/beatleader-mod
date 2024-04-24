using BeatLeader.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ReeWrapperV3<T> : ReactiveComponent where T : ReeUIComponentV3<T> {
        public T ReeComponent { get; private set; } = null!;
        
        protected override GameObject Construct() {
            ReeComponent = ReeUIComponentV3<T>.Instantiate(null!);
            return ReeComponent.Content;
        }

        protected override void OnInitialize() {
            Content.name = $"{typeof(T).Name} (Wrapped)";
        }
    }
    
    internal class ReeWrapperV2<T> : ReactiveComponent where T : ReeUIComponentV2 {
        public T ReeComponent { get; private set; } = null!;
        
        protected override GameObject Construct() {
            ReeComponent = ReeUIComponentV2.Instantiate<T>(null!);
            ReeComponent.ManualInit(null!);
            return ReeComponent.GetRootTransform().gameObject;
        }
        
        protected override void OnInitialize() {
            Content.name = $"{typeof(T).Name} (Wrapped)";
        }
    }
}
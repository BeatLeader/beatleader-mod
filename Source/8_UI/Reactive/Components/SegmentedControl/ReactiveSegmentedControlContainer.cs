using System;

namespace BeatLeader.UI.Reactive.Components {
    internal class ReactiveSegmentedControlContainer<TKey> : SegmentedControlContainer<TKey, ReactiveComponent> {
        public override Action<ReactiveComponent, bool> ActivationContract { get; set; } = (view, b) => view.Content.SetActive(b);
    }
}
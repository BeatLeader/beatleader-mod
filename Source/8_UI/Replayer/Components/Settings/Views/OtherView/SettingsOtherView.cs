using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsOtherView : ReactiveComponent {
        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new ScrollArea {
                        ScrollContent = new Dummy().WithSizeDelta(0f, 100f)
                    }.AsFlexItem(grow: 1f).Export(out var scrollArea),
                    //
                    new Scrollbar().With(x => scrollArea.Scrollbar = x).AsFlexItem()
                }
            }.AsFlexGroup().Use();
        }
    }
}
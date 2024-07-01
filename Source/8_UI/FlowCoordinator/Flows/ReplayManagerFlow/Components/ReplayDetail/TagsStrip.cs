using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class TagsStrip : ReactiveComponent {
        protected override GameObject Construct() {
            return new Image {
                Children = {
                    
                }
            }.AsFlexGroup().Use();
        }
    }
}
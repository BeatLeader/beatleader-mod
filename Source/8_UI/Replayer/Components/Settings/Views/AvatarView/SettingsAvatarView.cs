using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsAvatarView : ReactiveComponent {
        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Label {
                        Text = "Nothing to show here right now!"
                    }.WithRectExpand()
                }
            }.Use();
        }
    }
}
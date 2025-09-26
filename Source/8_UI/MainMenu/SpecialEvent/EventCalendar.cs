using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventCalendar : ReactiveComponent {
        protected override GameObject Construct() {
            return new Background()
                .AsBackground(color: Color.red.ColorWithAlpha(0.2f))
                .Use();
        }
    }
}
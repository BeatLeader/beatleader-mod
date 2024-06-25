using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IClickableComponent {
        event Action? ClickEvent;
    }
}
using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutItem {
        ILayoutDriver? Driver { get; set; }
        
        ILayoutModifier LayoutModifier { get; }
        RectTransform RectTransform { get; }

        event Action? ModifierUpdatedEvent;
    }
}
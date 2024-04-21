using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutItem {
        ILayoutDriver? LayoutDriver { get; set; }
        
        ILayoutModifier LayoutModifier { get; }
        RectTransform RectTransform { get; }
        
        float? DesiredHeight { get; }
        float? DesiredWidth { get; }

        event Action? ModifierUpdatedEvent;
    }
}
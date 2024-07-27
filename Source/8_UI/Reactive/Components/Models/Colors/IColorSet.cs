using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface IColorSet {
        event Action? SetUpdatedEvent;
        
        Color GetColor(GraphicElementState state);
    }
}
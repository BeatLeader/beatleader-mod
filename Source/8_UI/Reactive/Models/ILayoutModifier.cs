using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutModifier : ICopiable<ILayoutModifier> {
        Vector3 Scale { get; }
        Vector2? SizeDelta { get; }
        Vector2 AnchorMax { get; }
        Vector2 AnchorMin { get; }
        Vector2 Pivot { get; }

        event Action? ModifierUpdatedEvent;

        void Refresh();
    }
}
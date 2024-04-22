using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutModifier : ICopiable<ILayoutModifier>, IContextMember {
        Vector2 Pivot { get; }

        event Action? ModifierUpdatedEvent;

        void ReloadLayoutItem(ILayoutItem? item);
    }
}
using System;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutModifier : ICopiable<ILayoutModifier>, IContextMember {
        event Action? ModifierUpdatedEvent;

        void ReloadLayoutItem(ILayoutItem? item);
    }
}
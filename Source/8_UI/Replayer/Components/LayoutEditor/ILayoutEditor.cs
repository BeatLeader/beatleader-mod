using System;
using System.Collections.Generic;

namespace BeatLeader.Components {
    internal interface ILayoutEditor {
        IReadOnlyCollection<ILayoutComponent> LayoutComponents { get; }
        LayoutEditorMode Mode { get; set; }

        event Action<LayoutEditorMode>? ModeChangedEvent;

        void CancelChanges();
    }
}
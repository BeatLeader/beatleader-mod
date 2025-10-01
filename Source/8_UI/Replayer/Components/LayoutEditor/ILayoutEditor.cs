using System;
using System.Collections.Generic;

namespace BeatLeader.Components {
    internal interface ILayoutEditor {
        IReadOnlyCollection<ILayoutComponent> LayoutComponents { get; }
        LayoutEditorMode Mode { get; set; }
        LayoutEditorMode PreviousMode { get; }

        event Action<LayoutEditorMode>? ModeChangedEvent;
        event Action<ILayoutComponent>? ComponentAddedEvent;
        event Action<ILayoutComponent>? ComponentRemovedEvent;

        void CancelChanges();
    }
}
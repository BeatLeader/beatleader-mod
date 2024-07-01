using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface ITableCell<in TItem> {
        event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;

        void Init(TItem item);
        void OnCellStateChange(bool selected);
    }
}
using System;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class TableComponentCell<TItem> : ReactiveComponent, ITableCell<TItem> {
        #region TableCell

        event Action<ITableCell<TItem>, bool>? ITableCell<TItem>.CellAskedToChangeSelectionEvent {
            add => CellAskedToChangeSelectionEvent += value;
            remove => CellAskedToChangeSelectionEvent -= value;
        }

        private event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;
        private TItem? _item;
        
        void ITableCell<TItem>.Init(TItem item) {
            _item = item;
            OnInit(item);
        }

        void ITableCell<TItem>.OnCellStateChange(bool selected) {
            OnCellStateChange(selected);
        }

        #endregion

        #region Abstraction

        protected TItem Item => _item ?? throw new UninitializedComponentException("The cell was not initialized");
        
        protected virtual void OnInit(TItem item) { }
        
        protected virtual void OnCellStateChange(bool selected) { }
        
        protected void SelectSelf(bool select) {
            CellAskedToChangeSelectionEvent?.Invoke(this, select);
        }

        #endregion
    }
}
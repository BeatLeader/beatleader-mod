namespace BeatLeader.UI.Reactive.Components {
    internal interface IReactiveTableCell {
        void OnCellStateChange(bool selected, bool highlighted);
    }

    internal interface IReactiveTableCell<TItem> : IReactiveTableCell {
        void Init(TItem item, IModifiableListComponent<TItem> list);
    }
    
    internal abstract class ReactiveTableCell<TItem> : ReactiveComponent, IReactiveTableCell<TItem> {
        protected IModifiableListComponent<TItem>? List { get; private set; }
        public TItem? Item { get; private set; }

        public void Init(TItem item, IModifiableListComponent<TItem> list) {
            Item = item;
            List = list;
            Init(item);
        }

        protected virtual void Init(TItem item) { }
        public virtual void OnCellStateChange(bool selected, bool highlighted) { }
    }

    /// <summary>
    /// Cell for <c>ReactiveListComponentBase</c>
    /// </summary>
    internal sealed class ReactiveListComponentBaseCell : ListComponentBaseCell {
        public IReactiveTableCell? cellComponent;

        private void RefreshState() {
            cellComponent?.OnCellStateChange(selected, highlighted);
        }

        protected override void SelectionDidChange(TransitionType transitionType) => RefreshState();

        protected override void HighlightDidChange(TransitionType transitionType) => RefreshState();
    }

    /// <summary>
    /// Universal ReactiveComponent base for lists with reactive cells
    /// </summary>
    /// <typeparam name="TItem">Data type</typeparam>
    /// <typeparam name="TCellComponent">Cell component type</typeparam>
    internal abstract class ReactiveListComponentBase<TItem, TCellComponent> : ListComponentBase<TItem>
        where TCellComponent : ReactiveComponentBase, IReactiveTableCell<TItem>, new() {
        
        #region ConstructCell

        protected sealed override ListComponentBaseCell ConstructCell(TItem data) {
            if (DequeueReusableCell("ReeCell") is not ReactiveListComponentBaseCell cell) {
                var reactive = new TCellComponent();
                reactive.Use();
                cell = reactive.Content.AddComponent<ReactiveListComponentBaseCell>();
                cell.cellComponent = reactive;
                cell.reuseIdentifier = "ReeCell";
            }
            var cellComponent = (TCellComponent)cell.cellComponent!;
            cellComponent.Init(data, this);
            OnCellConstruct(cellComponent);
            return cell;
        }

        protected virtual void OnCellConstruct(TCellComponent cell) { }

        #endregion
    }
}
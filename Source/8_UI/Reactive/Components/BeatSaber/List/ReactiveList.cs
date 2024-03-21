namespace BeatLeader.UI.Reactive.Components {
    internal class ReactiveList<TItem, TReactive> : ReactiveListComponentBase<TItem, TReactive>
        where TReactive : ReactiveComponent, IReactiveTableCell<TItem>, new() {
        
        public ReactiveList(float cellSize) {
            CellSize = cellSize;
        }

        protected override float CellSize { get; }
    }
}
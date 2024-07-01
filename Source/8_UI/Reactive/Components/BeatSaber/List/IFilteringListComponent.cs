namespace BeatLeader.UI.Reactive.Components {
    internal interface IFilteringListComponent<T> {
        IListFilter<T>? Filter { get; set; }
    }
}
namespace BeatLeader.UI.Reactive.Components {
    internal interface IFilteringListComponent<T> : IListComponent<T> {
        IListFilter<T>? Filter { get; set; }
    }
}
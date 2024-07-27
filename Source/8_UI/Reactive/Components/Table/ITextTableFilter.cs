namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// A regular list filter with an ability to acquire the matched phrase.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    internal interface ITextTableFilter<in T> : ITableFilter<T> {
        string? GetMatchedPhrase(T value);
    }
}
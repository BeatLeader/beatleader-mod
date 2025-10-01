using Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal interface ITextTableFilter<in T> : ITableFilter<T> {
        string? GetMatchedPhrase(T value);
    }
}
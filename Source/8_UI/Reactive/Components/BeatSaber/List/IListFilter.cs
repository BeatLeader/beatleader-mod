using System;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// A list filter.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    internal interface IListFilter<in T> {
        event Action? FilterUpdatedEvent;
        
        bool Matches(T value);
    }
}
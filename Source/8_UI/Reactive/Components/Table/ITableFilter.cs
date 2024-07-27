using System;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// A table filter.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    internal interface ITableFilter<in T> {
        event Action? FilterUpdatedEvent;
        
        bool Matches(T value);
    }
}
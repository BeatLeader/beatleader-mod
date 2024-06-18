using System;
using System.Collections.Generic;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for UI elements that offer to select one item from a list
    /// </summary>
    /// <typeparam name="TKey">An item key</typeparam>
    internal interface IKeyedControlComponent<TKey> {
        TKey SelectedKey { get; }

        event Action<TKey>? SelectedKeyChangedEvent; 
        
        void Select(TKey key);
    }
    
    /// <summary>
    /// Abstraction for UI elements that offer to select one item from a list
    /// </summary>
    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    internal interface IKeyedControlComponent<TKey, TParam> : IKeyedControlComponent<TKey> {
        IDictionary<TKey, TParam> Items { get; }
    }
}
using System.Collections.Generic;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for UI elements that offer to select one item from a list
    /// </summary>
    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    public interface IKeyedControlComponent<TKey, TParam> {
        IDictionary<TKey, TParam> Items { get; }
        TKey SelectedKey { get; }

        void Select(TKey key);
    }
}
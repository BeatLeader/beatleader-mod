using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IKeyedControlComponentCell<TKey, in TParam> : IKeyedControlComponentCellBase<TKey, TParam> {
        event Action<TKey>? CellAskedToBeSelectedEvent;
        
        void OnCellStateChange(bool selected);
    }
}
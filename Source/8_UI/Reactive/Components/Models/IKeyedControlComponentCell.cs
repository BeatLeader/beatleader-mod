using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IKeyedControlComponentCell<TKey, in TParam> {
        event Action<TKey>? CellAskedToBeSelectedEvent;

        void Init(TKey key, TParam param);
        void OnCellStateChange(bool selected);
    }
}
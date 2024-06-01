using System;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class KeyedControlComponentCell<TKey, TParam> : ReactiveComponent, IKeyedControlComponentCell<TKey, TParam> {
        protected TKey Key => _key ?? throw new UninitializedComponentException();

        public event Action<TKey>? CellAskedToBeSelectedEvent;

        private TKey? _key;

        public void Init(TKey key, TParam param) {
            _key = key;
            OnInit(key, param);
        }

        public virtual void OnCellStateChange(bool selected) { }
        public virtual void OnInit(TKey key, TParam param) { }

        protected void SelectSelf() {
            CellAskedToBeSelectedEvent?.Invoke(Key);
        }
    }
}
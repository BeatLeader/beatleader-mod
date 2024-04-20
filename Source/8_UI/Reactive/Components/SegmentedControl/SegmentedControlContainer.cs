using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for the view source
    /// </summary>
    /// <typeparam name="TKey">View key</typeparam>
    internal interface ISegmentedControlDataSource<TKey> {
        IReadOnlyCollection<TKey> Items { get; }

        event Action? ItemsUpdatedEvent;

        void OnKeySelected(TKey key);
    }

    internal class SegmentedControlContainer<TKey, TView> : ReactiveComponent, ISegmentedControlDataSource<TKey> {
        #region DataSource Impl

        IReadOnlyCollection<TKey> ISegmentedControlDataSource<TKey>.Items => _items.Keys;

        public event Action? ItemsUpdatedEvent;

        void ISegmentedControlDataSource<TKey>.OnKeySelected(TKey key) => SelectKey(key);

        #endregion

        #region Items

        public IDictionary<TKey, TView> Items => _items;

        public virtual Action<TView, bool> ActivationContract {
            get => _activationContract ?? throw new UninitializedComponentException("Activation contract was not specified");
            set => _activationContract = value;
        }
        
        private readonly ObservableDictionary<TKey, TView> _items = new();
        private TView? _selectedView;
        private TKey? _selectedKey;
        private Action<TView, bool>? _activationContract;

        public void SelectKey(TKey key) {
            _selectedKey = key;
            Refresh();
        }

        private void Refresh() {
            if (_items.Count is 0) return;
            var view = _selectedKey is { } v ? _items[v] : _items.Values.First();
            SwitchObject(view);
        }

        private void SwitchObject(TView view) {
            if (_selectedView != null) {
                ActivationContract(_selectedView!, false);
            }
            _selectedView = view;
            ActivationContract(_selectedView!, true);
        }

        #endregion

        #region Setup

        protected override void OnInstantiate() {
            _items.ItemAddedEvent += HandleViewAdded;
            _items.ItemRemovedEvent += HandleViewRemoved;
            _items.AllItemsRemovedEvent += HandleAllViewsRemoved;
        }

        #endregion

        #region Callbacks

        private void HandleViewAdded(TKey key, TView view) {
            ActivationContract(view, false);
            Refresh();
            ItemsUpdatedEvent?.Invoke();
        }
        
        private void HandleViewRemoved(TKey key) {
            Refresh();
            ItemsUpdatedEvent?.Invoke();
        }
        
        private void HandleAllViewsRemoved() {
            Refresh();
            ItemsUpdatedEvent?.Invoke();
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.UI.Reactive.Yoga;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for segmented controls with selection ability
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    /// <typeparam name="TParam"></typeparam>
    internal interface ISegmentedControlComponent<TKey, TParam> {
        IDictionary<TKey, TParam> Items { get; }
        TKey SelectedKey { get; }

        void Select(TKey key);
    }

    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    /// <typeparam name="TCell">A cell component</typeparam>
    internal class SegmentedControl<TKey, TParam, TCell> : DrivingReactiveComponentBase, IKeyedControlComponent<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, IKeyedControlComponentCell<TKey, TParam>, new() {
        #region SegmentedControl

        public IDictionary<TKey, TParam> Items => _items;

        public TKey SelectedKey {
            get => _selectedKey ?? throw new InvalidOperationException("Key cannot be acquired when Items is empty");
            private set {
                if (value!.Equals(_selectedKey)) return;
                _selectedKey = value;
                SelectedKeyChangedEvent?.Invoke(value);
                NotifyPropertyChanged();
            }
        }

        public FlexDirection Direction {
            set => this.AsFlexGroup(direction: value);
        }

        public event Action<TKey>? SelectedKeyChangedEvent;

        private readonly ReactivePool<TKey, TCell> _cells = new();
        private readonly ObservableDictionary<TKey, TParam> _items = new();
        private TCell? _selectedCell;
        private TKey? _selectedKey;

        private void SpawnCell(TKey key) {
            var cell = _cells.Spawn(key);
            cell.AsFlexItem(grow: 1f);
            cell.Init(key, Items[key]);
            cell.CellAskedToBeSelectedEvent += HandleCellAskedToBeSelected;
            Children.Add(cell);
            OnCellConstruct(cell);
            if (_selectedCell == null) {
                Select(Items.Keys.First());
            }
        }

        private void DespawnCell(TKey key) {
            if (_selectedKey?.Equals(key) ?? false) {
                _selectedCell!.OnCellStateChange(false);
            }
            var cell = _cells.SpawnedComponents[key];
            cell.CellAskedToBeSelectedEvent -= HandleCellAskedToBeSelected;
            Children.Remove(cell);
            _cells.Despawn(cell);
        }

        public void Select(TKey key) {
            _selectedCell?.OnCellStateChange(false);
            _selectedCell = _cells.SpawnedComponents[key];
            _selectedCell.OnCellStateChange(true);
            SelectedKey = key;
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            Direction = FlexDirection.Row;
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
        }

        #endregion

        #region Abstraction

        protected virtual void OnCellConstruct(TCell cell) { }

        #endregion

        #region Callbacks

        private void HandleCellAskedToBeSelected(TKey key) {
            Select(key);
        }

        private void HandleItemAdded(TKey key, TParam param) {
            SpawnCell(key);
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleItemRemoved(TKey key) {
            DespawnCell(key);
            NotifyPropertyChanged(nameof(Items));
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for segmented controls
    /// </summary>
    internal interface ISegmentedControlComponent {
        void SelectKey(int idx);
    }

    /// <summary>
    /// Abstraction for segmented controls with selection ability
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    internal interface ISegmentedControlComponent<TKey> : ISegmentedControlComponent {
        ISegmentedControlDataSource<TKey>? DataSource { get; }

        void SelectKey(TKey key);
    }

    /// <summary>
    /// Cell base for <see cref="SegmentedControlComponentBase{TKey}"/>
    /// </summary>
    //Unity does not support generic components, so cell located not into the segmented control itself
    internal abstract class SegmentedControlComponentBaseCell : MonoBehaviour {
        public object? Key { get; private set; }

        private ISegmentedControlComponent? _segmentedControl;
        private int _idx;

        public void Init(ISegmentedControlComponent segmentedControl, object key, int idx) {
            _segmentedControl = segmentedControl;
            Key = key;
            _idx = idx;
        }

        public void SetState(bool state) {
            OnStateChange(state);
        }

        public object GetKeyOrThrow() {
            return Key ?? throw new UninitializedComponentException();
        }

        protected void CellSelectSelf() {
            _segmentedControl?.SelectKey(_idx);
        }

        protected virtual void OnStateChange(bool state) { }
    }

    /// <summary>
    /// Universal <see cref="ReactiveComponent"/> base for segmented controls
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    internal abstract class SegmentedControlComponentBase<TKey> : DrivingReactiveComponentBase, ISegmentedControlComponent<TKey> {
        #region Events

        public event Action<TKey>? CellWithKeySelectedEvent;

        #endregion

        #region SegmentedControl

        public ISegmentedControlDataSource<TKey>? DataSource {
            get => _dataSource;
            set {
                if (_dataSource != null) {
                    _dataSource.ItemsUpdatedEvent -= HandleDataSourceItemsUpdated;
                }
                _dataSource = value;
                if (_dataSource != null) {
                    _dataSource.ItemsUpdatedEvent += HandleDataSourceItemsUpdated;
                }
                Reload();
            }
        }

        private IReadOnlyCollection<TKey>? Items => DataSource?.Items;

        private readonly List<SegmentedControlComponentBaseCell> _reusableCells = new();
        private readonly List<SegmentedControlComponentBaseCell> _cells = new();
        private int _selectedCellIdx = -1;
        private ISegmentedControlDataSource<TKey>? _dataSource;

        public void Reload() {
            _reusableCells.AddRange(_cells);
            _cells.Clear();
            OnReload();
            if (Items != null) {
                GenerateCells();
                if (Items.Count > 0) {
                    SelectKey(0);
                }
            }
            foreach (var cell in _reusableCells) {
                cell.gameObject.SetActive(false);
            }
        }

        public void SelectKey(TKey key) {
            var idx = _cells.FindIndex(x => x.GetKeyOrThrow().Equals(key));
            if (idx is -1) return;
            SelectKey(idx);
        }

        public void SelectKey(int idx) {
            if (_selectedCellIdx != -1) {
                _cells[_selectedCellIdx].SetState(false);
            }
            _selectedCellIdx = idx;
            var newCell = _cells[_selectedCellIdx];
            newCell.SetState(true);
            var key = (TKey)newCell.GetKeyOrThrow();
            DataSource?.OnKeySelected(key);
            CellWithKeySelectedEvent?.Invoke(key);
        }
        
        protected SegmentedControlComponentBaseCell? DequeueReusableCell() {
            if (_reusableCells.Count is 0) return null;
            var val = _reusableCells[0];
            _reusableCells.RemoveAt(0);
            return val;
        }

        private void GenerateCells() {
            var idx = 0;
            foreach (var key in Items!) {
                var cell = ConstructCell(key);
                cell.Init(this, key!, idx);
                cell.transform.SetParent(ContentTransform);
                _cells.Add(cell);
                idx++;
            }
        }

        #endregion

        #region Abstraction
        
        protected abstract SegmentedControlComponentBaseCell ConstructCell(TKey key);

        protected virtual void OnReload() { }

        #endregion

        #region Callbacks

        private void HandleDataSourceItemsUpdated() {
            Reload();
        }

        #endregion
    }
}
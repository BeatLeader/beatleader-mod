using System;
using System.Collections.Generic;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    /// <summary>
    /// Abstraction for segmented controls
    /// </summary>
    internal interface ISegmentedControlComponent {
        void SelectItem(int idx);
    }

    /// <summary>
    /// Abstraction for segmented controls with selection ability
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    internal interface ISegmentedControlComponent<in TKey> : ISegmentedControlComponent {
        void SelectItem(TKey key);
    }

    /// <summary>
    /// Abstraction for segmented controls with items
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    /// <typeparam name="TValue">Item value</typeparam>
    internal interface ISegmentedControlComponent<TKey, TValue> : ISegmentedControlComponent<TKey> {
        ISegmentedControlDataSource<TKey, TValue>? DataSource { get; }
    }

    /// <summary>
    /// Cell base for <c>SegmentedControlComponentBase</c>
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

        protected void SelectSelf() {
            _segmentedControl?.SelectItem(_idx);
        }

        protected abstract void OnStateChange(bool state);
    }

    /// <summary>
    /// Universal ReeUIComponentV3 base for segmented controls
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    /// <typeparam name="TKey">Item key</typeparam>
    /// <typeparam name="TValue">Item value</typeparam>
    //TODO: rework to be dependent on the ISource which will be implemented by container
    internal abstract class SegmentedControlComponentBase<T, TKey, TValue> : LayoutComponentBase<T>, ISegmentedControlComponent<TKey, TValue>
        where T : ReeUIComponentV3<T> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<TKey>? CellWithKeySelectedEvent;

        #endregion

        #region SegmentedControl

        public ISegmentedControlDataSource<TKey, TValue>? DataSource { get; private set; }
        private IReadOnlyDictionary<TKey, TValue> Items => DataSource?.Items ?? throw new UninitializedComponentException();

        private readonly List<SegmentedControlComponentBaseCell> _reusableCells = new();
        private readonly List<SegmentedControlComponentBaseCell> _cells = new();
        private int _selectedCellIdx;

        public void Reload() {
            _reusableCells.AddRange(_cells);
            _cells.Clear();
            GenerateCells();
            if (Items.Count > 0) SelectItem(0);
        }

        public void SetDataSource(ISegmentedControlDataSource<TKey, TValue> source) {
            DataSource = source;
            Reload();
        }

        public void SelectItem(TKey key) {
            var idx = _cells.FindIndex(x => x.GetKeyOrThrow().Equals(key));
            if (idx is -1) return;
            SelectItem(idx);
        }

        public void SelectItem(int idx) {
            _cells[_selectedCellIdx].SetState(false);
            _selectedCellIdx = idx;
            var newCell = _cells[_selectedCellIdx];
            newCell.SetState(true);
            var key = (TKey)newCell.GetKeyOrThrow();
            DataSource?.OnItemSelect(key);
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
            foreach (var (key, value) in Items) {
                var cell = ConstructCell(value);
                cell.Init(this, key!, idx);
                cell.transform.SetParent(ContentTransform);
                _cells.Add(cell);
                idx++;
            }
        }

        #endregion

        #region Abstraction

        protected abstract SegmentedControlComponentBaseCell ConstructCell(TValue value);

        #endregion
    }
}
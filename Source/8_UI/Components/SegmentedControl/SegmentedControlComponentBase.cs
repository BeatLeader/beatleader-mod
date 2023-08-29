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
        void Reload();

        void SelectItem(int idx);
    }

    /// <summary>
    /// Abstraction for segmented controls with key
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    internal interface ISegmentedControlComponent<in TKey> : ISegmentedControlComponent {
        void SelectItem(TKey key);
    }

    /// <summary>
    /// Modifiable abstraction for segmented controls
    /// </summary>
    /// <typeparam name="TKey">Item key</typeparam>
    /// <typeparam name="TValue">Item value</typeparam>
    internal interface IModifiableSegmentedControlComponent<TKey, TValue> : ISegmentedControlComponent<TKey> {
        IDictionary<TKey, TValue> Items { get; }
    }

    /// <summary>
    /// Cell base for <c>SegmentedControlComponentBase</c>
    /// </summary>
    //Unity does not support generic components, so cell located not into the segmented control itself
    internal abstract class SegmentedControlComponentBaseCell : MonoBehaviour {
        public object Key { get; private set; } = default!;

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
        
        public abstract void OnStateChange(bool state);

        protected void NotifyControlStateChanged() {
            _segmentedControl?.SelectItem(_idx);
        }
    }

    /// <summary>
    /// Universal ReeUIComponentV3 base for segmented controls
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    /// <typeparam name="TKey">Item key</typeparam>
    /// <typeparam name="TValue">Item value</typeparam>
    internal abstract class SegmentedControlComponentBase<T, TKey, TValue> : LayoutComponentBase<T>, IModifiableSegmentedControlComponent<TKey, TValue>
        where T : ReeUIComponentV3<T> {

        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<TKey>? CellWithKeySelectedEvent;

        #endregion

        #region ModifiableSegmentedControlComponent

        IDictionary<TKey, TValue> IModifiableSegmentedControlComponent<TKey, TValue>.Items => items;

        #endregion

        #region SegmentedControl

        public readonly Dictionary<TKey, TValue> items = new();

        private readonly List<SegmentedControlComponentBaseCell> _reusableCells = new();
        private readonly List<SegmentedControlComponentBaseCell> _cells = new();
        private int _selectedCellIdx;

        public void Reload() {
            _reusableCells.AddRange(_cells);
            _cells.Clear();
            GenerateCells();
            if (items.Count > 0) SelectItem(0);
        }

        public void SelectItem(TKey key) {
            var idx = _cells.FindIndex(x => x.Key.Equals(key));
            if (idx is -1) return;
            SelectItem(idx);
        }

        public void SelectItem(int idx) {
            _cells[_selectedCellIdx].OnStateChange(false);
            _selectedCellIdx = idx;
            var newCell = _cells[_selectedCellIdx];
            newCell.OnStateChange(true);
            CellWithKeySelectedEvent?.Invoke((TKey)newCell.Key);
        }

        protected SegmentedControlComponentBaseCell? DequeueReusableCell() {
            if (_reusableCells.Count is 0) return null;
            var val = _reusableCells[0];
            _reusableCells.RemoveAt(0);
            return val;
        }

        private void GenerateCells() {
            var idx = 0;
            foreach (var (key, value) in items) {
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
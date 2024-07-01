using System;
using BeatLeader.UI.BSML_Addons;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface IReeSegmentedControlCell {
        event Action? SelectCellEvent;

        void OnCellStateChange(bool selected);
    }

    internal interface IReeSegmentedControlCell<TKey, TValue> : IReeSegmentedControlCell {
        void Init(TValue value, ISegmentedControlComponent<TKey, TValue> control);
    }

    [BSMLComponent(Suppress = true)]
    internal abstract class ReeSegmentedControlCell<T, TKey, TValue> : ReeUIComponentV3<T>, IReeSegmentedControlCell<TKey, TValue> where T : ReeUIComponentV3<T> {
        protected ISegmentedControlComponent<TKey, TValue>? Control { get; private set; }
        public TValue? Value { get; private set; }

        public event Action? SelectCellEvent;

        public void Init(TValue value, ISegmentedControlComponent<TKey, TValue> control) {
            Value = value;
            Control = control;
            Init(value);
        }

        protected virtual void Init(TValue value) { }
        public virtual void OnCellStateChange(bool selected) { }

        protected void SelectSelf() {
            SelectCellEvent?.Invoke();
        }
    }

    /// <summary>
    /// Cell for <c>ReeSegmentedControlComponentBase</c>
    /// </summary>
    internal sealed class ReeSegmentedControlBaseCell : SegmentedControlComponentBaseCell {
        public IReeSegmentedControlCell? cellComponent;

        public void Init() {
            cellComponent!.SelectCellEvent += SelectSelf;
        }

        private void OnDestroy() {
            cellComponent!.SelectCellEvent -= SelectSelf;
        }

        protected override void OnStateChange(bool state) {
            cellComponent?.OnCellStateChange(state);
        }
    }

    internal abstract class ReeSegmentedControlComponentBase<T, TCellComponent, TKey, TValue> : SegmentedControlComponentBase<T, TKey, TValue>
        where T : ReeUIComponentV3<T>
        where TCellComponent : ReeUIComponentV3<TCellComponent>, IReeSegmentedControlCell<TKey, TValue> {
        #region ConstructCell

        protected sealed override SegmentedControlComponentBaseCell ConstructCell(TValue value) {
            if (DequeueReusableCell() is not ReeSegmentedControlBaseCell cell) {
                var cellGo = new GameObject("Cell");
                cellGo.AddComponent<RectTransform>();
                cell = cellGo.AddComponent<ReeSegmentedControlBaseCell>();
                cell.cellComponent = ReeUIComponentV3<TCellComponent>.Instantiate(cellGo.transform);
                cell.Init();
            }
            var cellComponent = (TCellComponent)cell.cellComponent!;
            cellComponent.Init(value, this);
            OnCellConstruct(cellComponent);
            return cell;
        }

        protected virtual void OnCellConstruct(TCellComponent cell) { }

        #endregion
    }
}
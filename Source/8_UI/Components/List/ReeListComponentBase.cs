using BeatLeader.UI.BSML_Addons;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface IReeTableCell {
        void OnStateChange(bool selected, bool highlighted);
    }

    internal interface IReeTableCell<in TItem> : IReeTableCell {
        void Init(TItem item);
    }

    [BSMLComponent(Suppress = true)]
    internal abstract class ReeTableCell<T, TItem> : ReeUIComponentV3<T>, IReeTableCell<TItem> where T : ReeUIComponentV3<T> {
        public virtual void Init(TItem item) { }
        public virtual void OnStateChange(bool selected, bool highlighted) { }
    }

    /// <summary>
    /// Cell for <c>ReeListComponentBase</c>
    /// </summary>
    internal sealed class ReeListComponentBaseCell : ListComponentBaseCell {
        public IReeTableCell? cellComponent;

        private void RefreshState() {
            cellComponent?.OnStateChange(selected, highlighted);
        }

        protected override void SelectionDidChange(TransitionType transitionType) => RefreshState();

        protected override void HighlightDidChange(TransitionType transitionType) => RefreshState();
    }

    /// <summary>
    /// Universal ReeUIComponentV3 base for lists with ree cells
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    /// <typeparam name="TItem">Data type</typeparam>
    /// <typeparam name="TCellComponent">Cell component type</typeparam>
    internal abstract class ReeListComponentBase<T, TItem, TCellComponent> : ListComponentBase<T, TItem>
        where T : ReeUIComponentV3<T>
        where TCellComponent : ReeUIComponentV3<TCellComponent>, IReeTableCell<TItem> {
        
        #region ConstructCell

        protected sealed override ListComponentBaseCell ConstructCell(TItem data) {
            if (DequeueReusableCell("ReeCell") is not ReeListComponentBaseCell cell) {
                var cellGo = new GameObject("Cell");
                cell = cellGo.AddComponent<ReeListComponentBaseCell>();
                cell.cellComponent = ReeUIComponentV3<TCellComponent>.Instantiate(cellGo.transform);
                cell.reuseIdentifier = "ReeCell";
            }
            var cellComponent = (TCellComponent)cell.cellComponent!;
            cellComponent.Init(data);
            OnCellConstruct(cellComponent);
            return cell;
        }

        protected virtual void OnCellConstruct(TCellComponent cell) { }
        
        #endregion
    }
}
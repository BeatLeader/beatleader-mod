using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IReactiveSegmentedControlCell {
        event Action? CellSelectRequestedEvent;

        void OnCellStateChange(bool selected);
    }

    internal interface IReactiveSegmentedControlCell<in TKey> : IReactiveSegmentedControlCell {
        void Init(TKey key);
    }

    internal abstract class ReactiveSegmentedControlCell<TKey> : ReactiveComponent, IReactiveSegmentedControlCell<TKey> {
        public event Action? CellSelectRequestedEvent;

        public virtual void Init(TKey key) { }
        public virtual void OnCellStateChange(bool selected) { }

        protected void SelectSelf() {
            CellSelectRequestedEvent?.Invoke();
        }
    }

    /// <summary>
    /// Cell for <c>ReeSegmentedControlComponentBase</c>
    /// </summary>
    internal sealed class ReactiveSegmentedControlBaseCell : SegmentedControlComponentBaseCell {
        public IReactiveSegmentedControlCell? cellComponent;

        public void Init() {
            cellComponent!.CellSelectRequestedEvent += CellSelectSelf;
        }

        private void OnDestroy() {
            cellComponent!.CellSelectRequestedEvent -= CellSelectSelf;
        }

        protected override void OnStateChange(bool state) {
            cellComponent?.OnCellStateChange(state);
        }
    }

    internal class ReactiveSegmentedControl<TKey, TCellComponent> : SegmentedControlComponentBase<TKey>
        where TCellComponent : ReactiveComponentBase, IReactiveSegmentedControlCell<TKey>, new() {
        #region Layout

        public ILayoutModifier CellLayoutModifier {
            set {
                _cellLayoutModifier = value;
                ReloadCellLayoutModifier();
            }
        }

        private ILayoutModifier _cellLayoutModifier = new YogaModifier {
            FlexGrow = 1f
        };

        private readonly HashSet<ILayoutModifier> _layoutModifiers = new();

        private void ReloadCellLayoutModifier() {
            _layoutModifiers.Clear();
            foreach (var child in Children.OfType<TCellComponent>()) {
                ApplyModifierIfNeeded(child);
            }
        }
        
        private void ApplyModifierIfNeeded(ReactiveComponentBase component) {
            if (_layoutModifiers.Contains(component.LayoutModifier)) return;
            var mod = _cellLayoutModifier.CreateCopy();
            _layoutModifiers.Add(mod);
            component.LayoutModifier = mod;
        }

        #endregion

        #region ConstructCell

        protected sealed override SegmentedControlComponentBaseCell ConstructCell(TKey value) {
            if (DequeueReusableCell() is not ReactiveSegmentedControlBaseCell cell) {
                var component = new TCellComponent();
                component.Use();
                cell = component.Content.AddComponent<ReactiveSegmentedControlBaseCell>();
                cell.cellComponent = component;
                cell.Init();
            }
            var cellComponent = (TCellComponent)cell.cellComponent!;
            cellComponent.Init(value);
            ApplyModifierIfNeeded(cellComponent);
            Children.Add(cellComponent);
            OnCellConstruct(cellComponent);
            return cell;
        }

        protected virtual void OnCellConstruct(TCellComponent cell) { }

        protected override void OnReload() {
            Children.Clear();
        }

        #endregion
    }
}
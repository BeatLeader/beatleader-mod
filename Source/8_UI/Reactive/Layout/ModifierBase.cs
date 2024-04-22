using System;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract class ModifierBase<T> : ModifierBase where T : ILayoutModifier, new() {
        public override void CopyFrom(ILayoutModifier mod) {
            base.CopyFrom(mod);
            if (mod is T similar) CopyFromSimilar(similar);
        }

        public override ILayoutModifier CreateCopy() {
            var n = new T();
            n.CopyFrom(this);
            return n;
        }

        public virtual void CopyFromSimilar(T similar) { }
    }

    internal class ModifierBase : ILayoutModifier {
        #region Modifier

        public Vector2 Pivot { get; set; } = Vector2.one * 0.5f;

        protected bool SuppressRefresh { get; set; }

        protected ILayoutItem? LayoutItem { get; private set; }

        public event Action? ModifierUpdatedEvent;

        public void ReloadLayoutItem(ILayoutItem? item) {
            LayoutItem = item;
            OnLayoutItemUpdate();
        }

        protected virtual void OnLayoutItemUpdate() { }

        public void Refresh() {
            if (SuppressRefresh) return;
            ModifierUpdatedEvent?.Invoke();
        }

        public virtual void CopyFrom(ILayoutModifier mod) {
            Pivot = mod.Pivot;
        }

        public virtual ILayoutModifier CreateCopy() {
            var n = new ModifierBase();
            n.CopyFrom(this);
            return n;
        }

        #endregion

        #region Context

        public virtual Type? ContextType => null;

        public virtual object CreateContext() {
            throw new NotSupportedException();
        }

        public virtual void ProvideContext(object context) { }

        #endregion
    }
}
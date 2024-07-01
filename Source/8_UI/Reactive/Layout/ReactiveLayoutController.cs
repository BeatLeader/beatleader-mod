using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract class ReactiveLayoutController : ILayoutController {
        #region LayoutController

        protected Rect Rect { get; private set; }
        
        public event Action? LayoutControllerUpdatedEvent;

        public void ReloadDimensions(Rect controllerRect) {
            Rect = controllerRect;
        }

        protected void Refresh() {
            LayoutControllerUpdatedEvent?.Invoke();
        }

        public virtual void ReloadChildren(IEnumerable<ILayoutItem> children) { }

        public abstract void Recalculate(bool root);
        public abstract void ApplyChildren();
        public abstract void ApplySelf(ILayoutItem item);

        #endregion

        #region Context

        public virtual Type? ContextType => null;

        public virtual object CreateContext() {
            throw new NotSupportedException();
        }

        public virtual void ProvideContext(object? context) { }

        #endregion
    }
}
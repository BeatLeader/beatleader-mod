using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract class ReactiveLayoutController : ILayoutController {
        #region LayoutController

        protected Rect Rect { get; private set; }

        public void ReloadDimensions(Rect controllerRect) {
            Rect = controllerRect;
        }

        protected void Refresh() {
            Recalculate(true);
            Apply();
        }
        
        public virtual void ReloadChildren(IEnumerable<ILayoutItem> children) { }

        public abstract void Recalculate(bool root);
        public abstract void Apply();

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
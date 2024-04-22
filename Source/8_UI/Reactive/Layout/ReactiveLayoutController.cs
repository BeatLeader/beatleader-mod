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

        public abstract void Recalculate();
        public abstract void Apply();

        public virtual void ReloadChildren(IEnumerable<ILayoutItem> children) { }

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
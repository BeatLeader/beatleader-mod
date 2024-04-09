using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal abstract class ReactiveLayoutController : ILayoutController {
        protected Rect Rect { get; private set; }
        
        public void ReloadDimensions(Rect controllerRect) {
            Rect = controllerRect;
        }

        public abstract void Recalculate();
        
        public virtual void ReloadChildren(IEnumerable<ILayoutItem> children) { }
    }
}
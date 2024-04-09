using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutController {
        void ReloadChildren(IEnumerable<ILayoutItem> children);
        void ReloadDimensions(Rect controllerRect);
        void Recalculate();
    }
}
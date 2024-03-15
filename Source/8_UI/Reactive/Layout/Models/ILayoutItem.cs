using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutItem {
        ILayoutItem? Parent { get; }
        IEnumerable<ILayoutItem> Children { get; }

        ILayoutController? LayoutController { get; }
        ILayoutModifier LayoutModifier { get; }
        RectTransform RectTransform { get; }
    }
}
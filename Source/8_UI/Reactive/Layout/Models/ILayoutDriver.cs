using System.Collections.Generic;

namespace BeatLeader.UI.Reactive {
    internal interface ILayoutDriver {
        IEnumerable<ILayoutItem> Children { get; }
        ILayoutController? LayoutController { get; }

        void AppendChild(ILayoutItem comp);
        void TruncateChild(ILayoutItem comp);
    }
}
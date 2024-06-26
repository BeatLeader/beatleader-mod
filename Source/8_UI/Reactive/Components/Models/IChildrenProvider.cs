using System.Collections.Generic;

namespace BeatLeader.UI.Reactive {
    internal interface IChildrenProvider {
        ICollection<ILayoutItem> Children { get; }
    }
}
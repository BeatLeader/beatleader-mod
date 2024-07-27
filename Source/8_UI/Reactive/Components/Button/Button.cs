using System.Collections.Generic;

namespace BeatLeader.UI.Reactive.Components {
    internal class Button : ButtonBase, IChildrenProvider {
        public new ICollection<ILayoutItem> Children => base.Children;
    }
}
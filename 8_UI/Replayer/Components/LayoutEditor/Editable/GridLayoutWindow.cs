using UnityEngine;
using BeatLeader.Models;

namespace BeatLeader.Components {
    internal class GridLayoutWindow : LayoutWindow {
        public ILayoutGridModel gridModel;
        public Vector2 anchor;
        public bool adjustByGrid = true;

        protected override Vector2 ApplyPosition(Vector2 cursorPos) {
            return LayoutTool.MapPosition(cursorPos, _target.rect.size, anchor, gridModel, adjustByGrid);
        }
    }
}

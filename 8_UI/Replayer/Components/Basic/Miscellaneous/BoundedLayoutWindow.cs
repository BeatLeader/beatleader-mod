using UnityEngine;

namespace BeatLeader.Components {
    internal class BoundedLayoutWindow : LayoutWindow {
        public RectTransform? boundsRect;
        public Vector2 bounds;

        protected override Vector2 ApplyPosition(Vector2 cursorPos) {
            return LayoutMapper.ClampInZone(cursorPos, _target.rect.size, boundsRect?.rect.size ?? bounds);
        }
    }
}

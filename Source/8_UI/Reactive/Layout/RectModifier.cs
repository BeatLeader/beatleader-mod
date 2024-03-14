using UnityEngine;

namespace BeatLeader.UI.Reactive {
    internal class RectModifier : ModifierBase<RectModifier> {
        public static RectModifier Expand => new() {
            SizeDelta = Vector2.zero
        };
    }
}
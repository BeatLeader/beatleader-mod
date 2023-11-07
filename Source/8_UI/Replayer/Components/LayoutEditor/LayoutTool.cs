using UnityEngine;

namespace BeatLeader.Components {
    internal static class LayoutTool {
        public static Vector2 ApplyBorders(Vector2 pos, Vector2 componentSize, Vector2 areaSize) {
            var areaDiv = areaSize / 2;
            var sizeDiv = componentSize / 2;
            for (var i = 0; i < 2; i++) {
                pos[i] = Mathf.Clamp(pos[i], sizeDiv[i] - areaDiv[i], areaDiv[i] - sizeDiv[i]);
            }
            return pos;
        }
    }
}
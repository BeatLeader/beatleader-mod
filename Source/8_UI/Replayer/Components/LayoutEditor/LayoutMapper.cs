using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Components {
    internal static class LayoutMapper {
        private static Vector2 ConvertPosition(Vector2 pos, Vector2 elementSize, Vector2 anchor, bool invert = false) {
            elementSize /= 2;
            elementSize = invert ? -elementSize : elementSize;
            for (int i = 0; i < 2; i++) {
                pos[i] += MathUtils.Map(anchor[i], 0, 1, -elementSize[i], elementSize[i]);
            }
            return pos;
        }

        public static Vector2 ToAnchoredPosition(Vector2 pos, Vector2 elementSize, Vector2 anchor) {
            return ConvertPosition(pos, elementSize, anchor, false);
        }

        public static Vector2 ToActualPosition(Vector2 pos, Vector2 elementSize, Vector2 anchor) {
            return ConvertPosition(pos, elementSize, anchor, true);
        }

        public static Vector2 ClampInZone(Vector2 pos, Vector2 elementSize, Vector2 zoneSize) {
            zoneSize /= 2;
            elementSize /= 2;
            for (int i = 0; i < 2; i++) {
                var edgePos = Mathf.Abs(elementSize[i] - zoneSize[i]);
                pos[i] = Mathf.Clamp(pos[i], -edgePos, edgePos);
            }
            return pos;
        }

        public static Vector2 MapByGridUnclamped(Vector2 pos, Vector2 elementSize, Vector2 anchor, ILayoutGrid grid) {
            var zoneSize = grid.Size / 2;
            var lineThickness = grid.LineThickness;
            var cellSize = grid.CellSize + lineThickness;
            var posOffset = new Vector2(lineThickness, lineThickness * -1.5f);
            elementSize /= 2;
            for (int i = 0; i < 2; i++) {
                var sizeOffset = MathUtils.Map(anchor[i], 0, 1, elementSize[i], -elementSize[i]);
                var boundDiff = (zoneSize[i] % cellSize / 2) + posOffset[i];
                pos[i] -= ((pos[i] + boundDiff - sizeOffset) % cellSize) - boundDiff;
            }
            return pos;
        }
    }
}

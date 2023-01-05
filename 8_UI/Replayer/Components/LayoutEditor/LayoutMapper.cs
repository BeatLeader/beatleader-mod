using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Components {
    internal static class LayoutMapper {
        public static Vector2 ClampInZone(Vector2 pos, Vector2 elementSize, Vector2 zoneSize) {
            zoneSize /= 2;
            elementSize /= 2;
            for (int i = 0; i < 2; i++) {
                var edgePos = Mathf.Abs(elementSize[i] - zoneSize[i]);
                pos[i] = Mathf.Clamp(pos[i], -edgePos, edgePos);
            }
            return pos;
        }

        public static Vector2 MapByGrid(Vector2 pos, Vector2 elementSize, Vector2 anchor, ILayoutGrid grid) {
            pos = MapByGridUnclamped(pos, elementSize, anchor, grid);
            return ClampInZone(pos, anchor, grid.Size);
        }

        public static Vector2 MapByGridUnclamped(Vector2 pos, Vector2 elementSize, Vector2 anchor, ILayoutGrid grid) {
            var zoneSize = grid.Size / 2;
            var lineThickness = grid.LineThickness;
            var cellSize = grid.CellSize + lineThickness;
            var posOffset = new Vector2(lineThickness, -(lineThickness + lineThickness / 2));
            elementSize /= 2;
            for (int i = 0; i < 2; i++) {
                var sizeOffset = MathUtils.Map(anchor[i], 0, 1, elementSize[i], -elementSize[i]);
                var boundDiff = (zoneSize[i] % cellSize / 2) + posOffset[i];
                pos[i] -= ((pos[i] + boundDiff - sizeOffset) % cellSize) - boundDiff;
            }
            return pos;
        }

        public static Vector2 MapClamped(Vector2 pos, Vector2 elementSize, Vector2 zoneSize) {
            var absPos = MathUtils.ToAbsPos(pos, zoneSize);
            for (int i = 0; i < 2; i++) {
                var modZone = zoneSize[i] / 2 - elementSize[i];
                absPos[i] = Mathf.Clamp(absPos[i], -modZone, modZone);
            }
            return absPos;
        }
    }
}

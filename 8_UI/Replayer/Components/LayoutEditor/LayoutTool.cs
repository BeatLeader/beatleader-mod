using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Components {
    internal static class LayoutTool {
        public static Vector2 MapPosition(Vector2 cursorPos, Vector2 elementSize, 
            Vector2 anchor, ILayoutGridModel gridModel, bool adjustByGrid = true) {
            var windowOffset = elementSize / 2;
            var zoneSize = new Vector2(gridModel.Width, gridModel.Height) / 2;
            var size = gridModel.CellSize + gridModel.LineThickness;
            var difOffset = new Vector2(gridModel.LineThickness,
                -(gridModel.LineThickness + gridModel.LineThickness / 2));

            for (int i = 0; i < 2; i++) {
                if (adjustByGrid) {
                    var offset = MathUtils.Map(anchor[i], 0, 1, -windowOffset[i], windowOffset[i]);
                    var dif = (zoneSize[i] % size / 2) + (difOffset[i] / 2);
                    cursorPos[i] += offset;
                    cursorPos[i] -= ((cursorPos[i] - dif) % size) + offset;
                }
                cursorPos[i] = Mathf.Clamp(cursorPos[i], windowOffset[i] - zoneSize[i], zoneSize[i] - windowOffset[i]);
            }

            return cursorPos;
        }
    }
}

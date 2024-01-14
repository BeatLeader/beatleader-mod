using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal class ScoresTableLayoutHelper {
        #region Constants

        private const float RowWidth = 80.0f;
        private const float Spacing = 1.3f;
        private const float PadLeft = 2.0f;
        private const float PadRight = 2.0f;
        private const float AvailableWidth = RowWidth - PadLeft - PadRight;

        #endregion

        #region Cells Dictionary

        private readonly Dictionary<ScoreRowCellType, List<AbstractScoreRowCell>> _cells = new();

        private List<AbstractScoreRowCell> GetEntry(ScoreRowCellType cellType) {
            List<AbstractScoreRowCell> entry;

            if (_cells.ContainsKey(cellType)) {
                entry = _cells[cellType];
            } else {
                entry = new List<AbstractScoreRowCell>();
                _cells[cellType] = entry;
            }

            return entry;
        }

        #endregion

        #region RegisterCell

        public void RegisterCell(ScoreRowCellType cellType, AbstractScoreRowCell cell) {
            GetEntry(cellType).Add(cell);
        }

        #endregion

        #region RecalculateLayout

        private static bool IsFlexible(ScoreRowCellType cellType) {
            return cellType is ScoreRowCellType.Clans or ScoreRowCellType.Username or ScoreRowCellType.Modifiers;
        }

        public void RecalculateLayout(ScoreRowCellType mask) {
            UpdateNonFlexibleCells(mask, out var flexibleWidth);
            UpdateFlexibleCells(mask, flexibleWidth);
        }

        private void UpdateNonFlexibleCells(ScoreRowCellType mask, out float flexibleWidth) {
            flexibleWidth = AvailableWidth;

            var i = 0;
            foreach (var keyValuePair in _cells) {
                var cellType = keyValuePair.Key;
                if (IsFlexible(cellType)) continue;

                var list = keyValuePair.Value;
                var active = mask.HasFlag(cellType);

                foreach (var cell in list) {
                    cell.SetActive(active);
                }

                if (!active) continue;

                var maximalWidth = list.Max(cell => cell.GetPreferredWidth());

                foreach (var cell in list) {
                    cell.SetCellWidth(maximalWidth);
                }

                if (i++ > 0) flexibleWidth -= Spacing;
                flexibleWidth -= maximalWidth;
            }
        }

        private void UpdateFlexibleCells(ScoreRowCellType mask, float flexibleWidth) {
            var modifiersCells = GetEntry(ScoreRowCellType.Modifiers);
            var nameCells = GetEntry(ScoreRowCellType.Username);
            var clansCells = GetEntry(ScoreRowCellType.Clans);

            for (var i = 0; i < nameCells.Count; i++) {
                var remainingWidth = flexibleWidth;

                var clansCell = clansCells[i];
                if (mask.HasFlag(ScoreRowCellType.Clans) && !clansCell.isEmpty) {
                    var clansWidth = clansCell.GetPreferredWidth();
                    clansCell.SetActive(true);
                    clansCell.SetCellWidth(clansWidth);
                    remainingWidth -= clansWidth + Spacing;
                } else {
                    clansCell.SetActive(false);
                }

                var modifiersCell = modifiersCells[i];
                if (!mask.HasFlag(ScoreRowCellType.Modifiers)) {
                    modifiersCell.SetValue("");
                }

                var modifiersWidth = modifiersCell.GetPreferredWidth();
                remainingWidth -= modifiersWidth + Spacing;

                var nameCell = nameCells[i];
                if (!mask.HasFlag(ScoreRowCellType.Username)) {
                    nameCell.SetValue("");
                }

                var nameWidth = Mathf.Min(nameCell.GetPreferredWidth(), remainingWidth);
                nameCell.SetCellWidth(nameWidth);
                remainingWidth -= nameWidth;

                if (remainingWidth > 0) modifiersWidth += remainingWidth;
                modifiersCell.SetCellWidth(modifiersWidth);
            }
        }

        #endregion
    }
}
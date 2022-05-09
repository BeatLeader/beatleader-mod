using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;

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

        public void RecalculateLayout(ScoreRowCellType mask) {
            UpdateNonFlexibleCells(mask, out var flexibleWidth);
            UpdateFlexibleCells(mask, flexibleWidth);
        }

        private void UpdateNonFlexibleCells(ScoreRowCellType mask, out float flexibleWidth) {
            flexibleWidth = AvailableWidth;

            var i = 0;
            foreach (var (cellType, list) in _cells) {
                var active = mask.HasFlag(cellType);

                foreach (var cell in list) {
                    cell.SetActive(!cell.isEmpty && active);
                }

                if (!active || cellType is ScoreRowCellType.Clans or ScoreRowCellType.Username) continue;

                var maximalWidth = list.Max(cell => cell.GetPreferredWidth());

                foreach (var cell in list) {
                    cell.SetCellWidth(maximalWidth);
                }

                if (i++ > 0) flexibleWidth -= Spacing;
                flexibleWidth -= maximalWidth;
            }
        }

        private void UpdateFlexibleCells(ScoreRowCellType mask, float flexibleWidth) {
            var nameCells = GetEntry(ScoreRowCellType.Username);
            var clansCells = GetEntry(ScoreRowCellType.Clans);

            for (var i = 0; i < nameCells.Count; i++) {
                var remainingWidth = flexibleWidth;

                if (mask.HasFlag(ScoreRowCellType.Clans)) {
                    var clansCell = clansCells[i];
                    if (!clansCell.isEmpty) {
                        var clansCellWidth = clansCell.GetPreferredWidth();
                        clansCell.SetCellWidth(clansCellWidth);
                        remainingWidth -= clansCellWidth + Spacing;
                    }
                }

                nameCells[i].SetCellWidth(remainingWidth);
            }
        }

        #endregion
    }
}
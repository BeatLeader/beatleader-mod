using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;

namespace BeatLeader {
    internal class ScoresTableLayoutHelper {
        #region Constants

        private const float RowWidth = 80.0f;
        private const float Spacing = 2.0f;
        private const float PadLeft = 2.0f;
        private const float PadRight = 2.0f;
        private const float AvailableWidth = RowWidth - PadLeft - PadRight;

        private const ScoreRowCellType FlexibleCellType = ScoreRowCellType.Username;

        //TODO Control via VisibilitySelector
        private const ScoreRowCellType Mask = ScoreRowCellType.Rank |
                                              ScoreRowCellType.Country |
                                              ScoreRowCellType.Avatar |
                                              ScoreRowCellType.Username |
                                              ScoreRowCellType.Modifiers |
                                              ScoreRowCellType.Accuracy |
                                              ScoreRowCellType.PerformancePoints |
                                              ScoreRowCellType.Score |
                                              ScoreRowCellType.Mistakes;

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

        public void RecalculateLayout(bool hasPP) {
            var flexibleWidth = AvailableWidth;

            var i = 0;
            foreach (var (cellType, list) in _cells) {
                var active = Mask.HasFlag(cellType);

                if (cellType == ScoreRowCellType.PerformancePoints) {
                    active &= hasPP;
                }

                foreach (var cell in list) {
                    cell.SetActive(active);
                }

                if (!active || cellType == FlexibleCellType) continue;

                var maximalWidth = list.Max(cell => cell.GetPreferredWidth());

                foreach (var cell in list) {
                    cell.SetCellWidth(maximalWidth);
                }

                if (i++ > 0) flexibleWidth -= Spacing;
                flexibleWidth -= maximalWidth;
            }

            foreach (var cell in GetEntry(FlexibleCellType)) {
                cell.SetCellWidth(flexibleWidth);
            }
        }

        #endregion
    }
}
using BeatLeader.Models;

namespace BeatLeader {
    public interface IScoreRowContent {
        bool ContainsValue(ScoreRowCellType cellType);
        object? GetValue(ScoreRowCellType cellType);
    }
}
using BeatLeader.Models;

namespace BeatLeader {
    internal static class ConfigDefaults {
        #region Enabled

        public const bool Enabled = true;

        #endregion

        #region ScoresContext

        public static readonly ScoresContext ScoresContext = ScoresContext.Modifiers;

        #endregion

        #region LeaderboardTableMask

        public const ScoreRowCellType LeaderboardTableMask = ScoreRowCellType.Rank |
                                              ScoreRowCellType.Username |
                                              ScoreRowCellType.Modifiers |
                                              ScoreRowCellType.Accuracy |
                                              ScoreRowCellType.PerformancePoints |
                                              ScoreRowCellType.Score;

        #endregion
    }
}
using BeatLeader.Models;

namespace BeatLeader
{
    internal static class ConfigDefaults
    {
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

        #region ReplayerSettings

        public static ReplayerSettings ReplayerSettings = new()
        {
            ShowUI = true,
            ForceUseReplayerCamera = false,
            LoadPlayerEnvironment = false,

            ShowHead = false,
            ShowLeftSaber = true,
            ShowRightSaber = true,

            MaxFOV = 110,
            MinFOV = 70,
            CameraFOV = 90,
            FPFCCameraPose = "PlayerView",
            VRCameraPose = "BehindView",
        };

        public static FloatingConfig FloatingConfig = new()
        {
            Position = new Vector3(0, 1, 2),
            Rotation = UnityEngine.Quaternion.Euler(new Vector3(40, 0, 0)),
            GridPosIncrement = 0.2f,
            GridRotIncrement = 5,
            IsPinned = true
        };

        #endregion

        #region EnableReplayCaching

        public static bool EnableReplayCaching = false;

        #endregion
    }
}
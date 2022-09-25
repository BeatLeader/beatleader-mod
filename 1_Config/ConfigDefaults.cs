using BeatLeader.Models;
using UnityEngine;

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

        public static readonly ReplayerSettings ReplayerSettings = new()
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

            Shortcuts = new()
            {
                HideUIHotkey = KeyCode.H,
                HideCursorHotkey = KeyCode.C,
                PauseHotkey = KeyCode.Space,
                SwitchViewRightHotkey = KeyCode.RightArrow,
                SwitchViewLeftHotkey = KeyCode.LeftArrow,
                IncFOVHotkey = KeyCode.UpArrow,
                DecFOVHotkey = KeyCode.DownArrow
            }
        };

        public static readonly FloatingConfig FloatingConfig = new()
        {
            Position = new UnityEngine.Vector3(0, 1, 2),
            Rotation = UnityEngine.Quaternion.Euler(new UnityEngine.Vector3(40, 0, 0)),
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
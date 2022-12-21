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

        public static ReplayerSettings ReplayerSettings => new()
        {
            AutoHideUI = false,
            LoadPlayerEnvironment = false,

            ShowHead = false,
            ShowLeftSaber = true,
            ShowRightSaber = true,
            ShowWatermark = true,

            MaxFOV = 110,
            MinFOV = 70,
            CameraFOV = 90,
            FPFCCameraPose = "PlayerView",
            VRCameraPose = "BehindView",

            Shortcuts = new()
            {
                LayoutEditorHotkey = KeyCode.H,
                HideCursorHotkey = KeyCode.C,
                PauseHotkey = KeyCode.Space,
                RewindForwardHotkey = KeyCode.RightArrow,
                RewindBackwardHotkey = KeyCode.LeftArrow,
            }
        };

        #endregion

        #region EnableReplayCaching

        public static bool EnableReplayCaching = false;

        #endregion
    }
}
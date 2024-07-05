using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal static class ConfigDefaults {
        #region Enabled

        public const bool Enabled = true;

        #endregion

        #region MenuButtonEnabled

        public const bool MenuButtonEnabled = true;

        #endregion

        #region BeatLeaderServer

        public const BeatLeaderServer MainServer = BeatLeaderServer.XYZ_DOMAIN;

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

        #region LeaderboardDisplaySettings

        public static LeaderboardDisplaySettings LeaderboardDisplaySettings = new() {
            ClanCaptureDisplay = true
        };

        #endregion

        #region ReplayerSettings

        public static readonly ReplayerSettings ReplayerSettings = new() {
            LoadPlayerEnvironment = false,
            ExitReplayAutomatically = true,

            ShowWatermark = true,
            ShowTimelineMisses = true,
            ShowTimelineBombs = true,
            ShowTimelinePauses = true,

            CameraSettings = new InternalReplayerCameraSettings {
                MaxCameraFOV = 110,
                MinCameraFOV = 70,
                CameraFOV = 90,
                FpfcCameraView = "PlayerView",
                VRCameraView = "BehindView"
            },

            BodySettings = {
                BodyModels = {
                    {
                        "Primary", SerializableVirtualPlayerBodyConfig.CreateManual(
                            new Dictionary<string, SerializableVirtualPlayerBodyPartConfig> {
                                {
                                    "BATTLE_ROYALE_AVATAR_HEAD", new() {
                                        PotentiallyActive = false,
                                        Alpha = 1f
                                    }
                                }, {
                                    "BATTLE_ROYALE_AVATAR_BODY", new() {
                                        PotentiallyActive = false,
                                        Alpha = 1f
                                    }
                                }, {
                                    "BATTLE_ROYALE_AVATAR_LEFT_HAND", new() {
                                        PotentiallyActive = true,
                                        Alpha = 1f
                                    }
                                }, {
                                    "BATTLE_ROYALE_AVATAR_RIGHT_HAND", new() {
                                        PotentiallyActive = true,
                                        Alpha = 1f
                                    }
                                }, {
                                    "LEFT_SABER", new() {
                                        PotentiallyActive = true,
                                        Alpha = 1f
                                    }
                                }, {
                                    "RIGHT_SABER", new() {
                                        PotentiallyActive = true,
                                        Alpha = 1f
                                    }
                                }
                            }
                        )
                    }, {
                        "Default", SerializableVirtualPlayerBodyConfig.CreateManual(
                            new Dictionary<string, SerializableVirtualPlayerBodyPartConfig> {
                                {
                                    "BATTLE_ROYALE_AVATAR_HEAD", new() {
                                        PotentiallyActive = false,
                                        Alpha = 1f
                                    }
                                }, {
                                    "BATTLE_ROYALE_AVATAR_BODY", new() {
                                        PotentiallyActive = false,
                                        Alpha = 1f
                                    }
                                }, {
                                    "BATTLE_ROYALE_AVATAR_LEFT_HAND", new() {
                                        PotentiallyActive = false,
                                        Alpha = 1f
                                    }
                                }, {
                                    "BATTLE_ROYALE_AVATAR_RIGHT_HAND", new() {
                                        PotentiallyActive = false,
                                        Alpha = 1f
                                    }
                                }, {
                                    "LEFT_SABER", new() {
                                        PotentiallyActive = true,
                                        Alpha = 0.7f
                                    }
                                }, {
                                    "RIGHT_SABER", new() {
                                        PotentiallyActive = true,
                                        Alpha = 0.7f
                                    }
                                }
                            }
                        )
                    }
                }
            },

            UISettings = {
                AutoHideUI = false,
                FloatingSettings = new() {
                    Pose = new() {
                        position = new(0f, 1f, 2f),
                        rotation = Quaternion.Euler(30f, 0f, 0f)
                    },
                    InitialPose = new() {
                        position = new(0f, 1f, 2f),
                        rotation = Quaternion.Euler(30f, 0f, 0f)
                    },
                    Pinned = true,
                    SnapEnabled = true,
                    CurvatureRadius = 90f,
                    CurvatureEnabled = true
                },
                LayoutEditorSettings = new() {
                    ComponentDatas = new() {
                        {
                            "Toolbar", new() {
                                position = new() {
                                    x = 3f,
                                    y = -96f
                                },
                                size = new() {
                                    x = 108f,
                                    y = 72f
                                },
                                layer = 1,
                                active = true
                            }
                        },
                        {
                            "Beatmap Preview", new() {
                                position = new() {
                                    x = -165f,
                                    y = 120f
                                },
                                size = new() {
                                    x = 84f,
                                    y = 24f
                                },
                                layer = 2,
                                active = true
                            }
                        },
                        {
                            "Player List", new() {
                                position = new() {
                                    x = -165f,
                                    y = 78f
                                },
                                size = new() {
                                    x = 84f,
                                    y = 60f
                                },
                                layer = 3,
                                active = true
                            }
                        }
                    }
                }
            },

            Shortcuts = {
                LayoutEditorPartialModeHotkey = KeyCode.H,
                HideCursorHotkey = KeyCode.C,
                PauseHotkey = KeyCode.Space,
                RewindForwardHotkey = KeyCode.RightArrow,
                RewindBackwardHotkey = KeyCode.LeftArrow,
            }
        };

        #endregion

        #region ReplaySavingSettings

        public const bool EnableReplayCaching = false;

        public const bool OverrideOldReplays = true;

        public const bool SaveLocalReplays = true;

        public const ReplaySaveOption ReplaySavingOptions = ReplaySaveOption.Exit | ReplaySaveOption.Fail | ReplaySaveOption.ZeroScore;

        #endregion

        #region Language

        public const BLLanguage SelectedLanguage = BLLanguage.GameDefault;

        #endregion
    }
}
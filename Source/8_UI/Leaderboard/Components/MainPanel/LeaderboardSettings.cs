using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LeaderboardSettings : AbstractReeModal<object> {
        #region Init / Dispose

        protected override void OnInitialize() {
            base.OnInitialize();
            ApplyScale();
        }

        protected override void OnDispose() { }

        #endregion

        #region Container

        private const float Scale = 0.8f;

        [UIComponent("container"), UsedImplicitly]
        private RectTransform _containerTransform;

        private void ApplyScale() {
            _containerTransform.localScale = new Vector3(Scale, Scale, Scale);
        }

        #endregion

        #region Toggles

        [UIValue("avatar-mask-value"), UsedImplicitly]
        private bool AvatarMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Avatar);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Avatar;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Avatar;
                }
            }
        }

        [UIValue("country-mask-value"), UsedImplicitly]
        private bool CountryMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Country);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Country;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Country;
                }
            }
        }

        [UIValue("clans-mask-value"), UsedImplicitly]
        private bool ClansMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Clans);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Clans;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Clans;
                }
            }
        }

        [UIValue("score-mask-value"), UsedImplicitly]
        private bool ScoreMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Score);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Score;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Score;
                }
            }
        }

        [UIValue("time-mask-value"), UsedImplicitly]
        private bool TimeMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Time);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Time;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Time;
                }
            }
        }

        [UIValue("mistakes-mask-value"), UsedImplicitly]
        private bool MistakesMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Mistakes);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Mistakes;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Mistakes;
                }
            }
        }

        [UIValue("pauses-mask-value"), UsedImplicitly]
        private bool PausesMaskValue {
            get => PluginConfig.LeaderboardTableMask.HasFlag(ScoreRowCellType.Pauses);
            set {
                if (value) {
                    PluginConfig.LeaderboardTableMask |= ScoreRowCellType.Pauses;
                } else {
                    PluginConfig.LeaderboardTableMask &= ~ScoreRowCellType.Pauses;
                }
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.UI;
using BeatLeader.UI.Hub;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UIPatches;
using BeatLeader.Utils;
using ModestTree;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class LeaderboardInfoPanel : ReactiveComponent {
        #region Construct

        private ReeWrapperV2<QualificationCheckbox> _criteriaCheckbox = null!;
        private ReeWrapperV2<QualificationCheckbox> _approvalCheckbox = null!;
        private CaptorClan _captorClan = null!;
        private MapStatus _mapStatus = null!;

        private DownloadScoresModal _downloadModal = null!;
        private Label _replaysLabel = null!;
        private ButtonBase _proceedButton = null!;

        private ImageButton _menuButton = null!;
        private PushContainer _container = null!;

        private static ImageButton CreateHeaderButton(Sprite sprite, Action callback) {
            return new ImageButton {
                Image = {
                    Sprite = sprite,
                    Material = BundleLoader.UIAdditiveGlowMaterial
                },
                Colors = UIStyle.ButtonColorSet,
                OnClick = callback
            }.AsFlexItem(size: 4f);
        }

        protected override GameObject Construct() {
            _downloadModal = new DownloadScoresModal {
                DownloadingFinishedCallback = OnScoreDownloadingFinished
            }.WithAlphaAnimation(() => Canvas!.gameObject).WithJumpAnimation();

            return new PushContainer {
                OpenedView = new Dummy {
                    Children = {
                        new Dummy {
                            Children = {
                                CreateHeaderButton(
                                    BundleLoader.Sprites.homeIcon,
                                    LeaderboardEvents.NotifyMenuButtonWasPressed
                                ).Bind(ref _menuButton),
                            }
                        }.AsFlexItem(grow: 1f).AsFlexGroup(justifyContent: Justify.FlexStart),

                        new Dummy {
                            Children = {
                                new ReeWrapperV2<MapStatus>().BindRee(ref _mapStatus),

                                new ReeWrapperV2<CaptorClan>().BindRee(ref _captorClan),

                                new ReeWrapperV2<QualificationCheckbox>().Bind(ref _criteriaCheckbox),

                                new ReeWrapperV2<QualificationCheckbox>().Bind(ref _approvalCheckbox),
                            }
                        }.With(
                            x => {
                                var group = x.Content.AddComponent<HorizontalLayoutGroup>();
                                group.childForceExpandWidth = false;
                                group.childAlignment = TextAnchor.MiddleCenter;
                                group.spacing = 1f;

                                var fitter = x.Content.AddComponent<ContentSizeFitter>();
                                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                            }
                        ),

                        new Dummy {
                            Children = {
                                CreateHeaderButton(
                                    BundleLoader.BattleRoyaleIcon,
                                    () => SetBattleRoyaleEnabled(true)
                                ),

                                CreateHeaderButton(
                                    BundleLoader.ProfileIcon,
                                    () => {
                                        if (_websiteLink == null) return;
                                        EnvironmentUtils.OpenBrowserPage(_websiteLink);
                                    }
                                ),

                                CreateHeaderButton(
                                    BundleLoader.SettingsIcon,
                                    LeaderboardEvents.NotifyLeaderboardSettingsButtonWasPressed
                                )
                            }
                        }.AsFlexItem(grow: 1f).AsFlexGroup(justifyContent: Justify.FlexEnd, gap: 1f)
                    }
                }.AsFlexGroup(
                    alignItems: Align.Center,
                    padding: new() { left = 1f, right = 2f },
                    gap: 1f
                ).WithRectExpand(),

                ClosedView = new Dummy {
                    Children = {
                        new BsButton {
                            ShowUnderline = false,
                            OnClick = () => SetBattleRoyaleEnabled(false)
                        }.WithLabel("Cancel").AsFlexItem(),

                        new Label {
                            FontSize = 5f
                        }.AsFlexItem().Bind(ref _replaysLabel),

                        new BsButton {
                                ShowUnderline = false,
                                OnClick = () => {
                                    ModalSystem.PresentModal(_downloadModal, Canvas!.transform);
                                    _downloadModal.StartDownloading(_selectedScores);
                                }
                            }
                            .WithLabel("Proceed")
                            .AsFlexItem()
                            .Bind(ref _proceedButton),
                    }
                }.AsFlexGroup(
                    alignItems: Align.Center,
                    padding: new() { left = 1f, right = 2f },
                    justifyContent: Justify.SpaceBetween
                ).WithRectExpand(),

                Opened = true,
                Color = Color.clear
            }.Bind(ref _container).Use();
        }

        #endregion

        #region Init/Dispose

        private ReplayerViewNavigatorWrapper? _replayerNavigator;

        public void Setup(ReplayerViewNavigatorWrapper navigator) {
            _replayerNavigator = navigator;
        }

        protected override void OnInitialize() {
            LeaderboardsCache.CacheWasChangedEvent += OnCacheWasChanged;
            PluginConfig.LeaderboardDisplaySettingsChangedEvent += OnLeaderboardDisplaySettingsChanged;
            EnvironmentManagerPatch.EnvironmentTypeChangedEvent += OnMenuEnvironmentChanged;
            LeaderboardEvents.ScoreInfoButtonWasPressed += OnScoreClicked;

            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        protected override void OnDestroy() {
            LeaderboardsCache.CacheWasChangedEvent -= OnCacheWasChanged;
            EnvironmentManagerPatch.EnvironmentTypeChangedEvent -= OnMenuEnvironmentChanged;
            PluginConfig.LeaderboardDisplaySettingsChangedEvent -= OnLeaderboardDisplaySettingsChanged;
            LeaderboardEvents.ScoreInfoButtonWasPressed -= OnScoreClicked;

            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
        }

        #endregion

        #region Battle Royale

        private readonly HashSet<Score> _selectedScores = new();
        private bool _battleRoyaleEnabled;

        private void SetBattleRoyaleEnabled(bool enabled) {
            _battleRoyaleEnabled = enabled;
            _container.Opened = !enabled;
            LeaderboardEvents.NotifyBattleRoyaleEnabled(enabled);

            if (enabled) {
                _selectedScores.Clear();
                RefreshBattleRoyaleUI();
            }
        }

        private void RefreshBattleRoyaleUI() {
            _replaysLabel.Text = $"{_selectedScores.Count} OPPONENTS";
            _proceedButton.Interactable = _selectedScores.Count > 1;
        }

        private void OnScoreClicked(Score score) {
            if (!_battleRoyaleEnabled) {
                return;
            }

            if (_selectedScores.Contains(score)) {
                _selectedScores.Remove(score);
            } else {
                _selectedScores.Add(score);
            }

            RefreshBattleRoyaleUI();
        }

        private void OnScoreDownloadingFinished() {
            var level = new BeatmapLevelWithKey(
                LeaderboardState.SelectedBeatmapLevel,
                LeaderboardState.SelectedBeatmapKey
            );

            _replayerNavigator?.NavigateToBattleRoyale(level, _downloadModal.Headers);
        }

        #endregion

        #region Events

        private void OnLeaderboardDisplaySettingsChanged(LeaderboardDisplaySettings settings) {
            _displayCaptorClan = settings.ClanCaptureDisplay;
            UpdateVisuals();
        }

        private void OnMenuEnvironmentChanged(MenuEnvironmentManager.MenuEnvironmentType type) {
            UpdateVisuals();
        }

        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
            _selectedScores.Clear();
            RefreshBattleRoyaleUI();
            SetBeatmap(key);
        }

        private void OnCacheWasChanged() {
            SetBeatmap(LeaderboardState.SelectedBeatmapKey);
        }

        #endregion

        #region SetBeatmap

        private RankedStatus _rankedStatus;
        private DiffInfo _difficultyInfo;
        private bool _displayCaptorClan = PluginConfig.LeaderboardDisplaySettings.ClanCaptureDisplay;
        private string? _websiteLink;

        private void SetBeatmap(BeatmapKey beatmap) {
            if (!beatmap.IsValid()) {
                _rankedStatus = RankedStatus.Unknown;
                _websiteLink = null;
                UpdateVisuals();
                return;
            }

            var key = LeaderboardKey.FromBeatmap(beatmap);
            if (!LeaderboardsCache.TryGetLeaderboardInfo(key, out var data)) {
                _rankedStatus = RankedStatus.Unknown;
                _websiteLink = null;
                UpdateVisuals();
                return;
            }

            _difficultyInfo = data.DifficultyInfo;
            _rankedStatus = FormatUtils.GetRankedStatus(data.DifficultyInfo);
            _websiteLink = BLConstants.LeaderboardPage(data.LeaderboardId);
            if (_rankedStatus is RankedStatus.Ranked) {
                _captorClan.SetValues(data);
            }

            UpdateCheckboxes(data.QualificationInfo);
            UpdateVisuals();
        }

        #endregion

        #region UpdateCheckboxes

        private void UpdateCheckboxes(QualificationInfo qualificationInfo) {
            string criteriaPostfix;

            if (qualificationInfo.criteriaCommentary == null || qualificationInfo.criteriaCommentary.IsEmpty()) {
                criteriaPostfix = "";
            } else {
                criteriaPostfix = $"<size=80%>\n\n{qualificationInfo.criteriaCommentary}";
            }

            var criteriaCheckbox = _criteriaCheckbox.ReeComponent;
            var approvalCheckbox = _approvalCheckbox.ReeComponent;

            switch (qualificationInfo.criteriaMet) {
                case 1:
                    criteriaCheckbox.SetState(QualificationCheckbox.State.Checked);
                    criteriaCheckbox.HoverHint = $"Criteria passed{criteriaPostfix}";
                    break;
                case 2:
                    criteriaCheckbox.SetState(QualificationCheckbox.State.Failed);
                    criteriaCheckbox.HoverHint = $"Criteria failed{criteriaPostfix}";
                    break;
                case 3:
                    criteriaCheckbox.SetState(QualificationCheckbox.State.OnHold);
                    criteriaCheckbox.HoverHint = $"Criteria on hold{criteriaPostfix}";
                    break;
                default:
                    criteriaCheckbox.SetState(QualificationCheckbox.State.Neutral);
                    criteriaCheckbox.HoverHint = $"Awaiting criteria check{criteriaPostfix}";
                    break;
            }

            if (qualificationInfo.approved) {
                approvalCheckbox.SetState(QualificationCheckbox.State.Checked);
                approvalCheckbox.HoverHint = "Qualified!";
            } else {
                approvalCheckbox.SetState(QualificationCheckbox.State.Neutral);
                approvalCheckbox.HoverHint = "Awaiting RT approval";
            }
        }

        #endregion

        #region UpdateVisuals

        private void UpdateVisuals() {
            _mapStatus.SetActive(_rankedStatus is not RankedStatus.Unknown);
            _mapStatus.SetValues(_rankedStatus, _difficultyInfo);
            _captorClan.SetActive(_displayCaptorClan && _rankedStatus is RankedStatus.Ranked);

            var qualificationActive = _rankedStatus is RankedStatus.Nominated or RankedStatus.Qualified or RankedStatus.Unrankable;
            _criteriaCheckbox.Enabled = qualificationActive;
            _approvalCheckbox.Enabled = qualificationActive;

            _menuButton.Enabled = EnvironmentManagerPatch.EnvironmentType is not MenuEnvironmentManager.MenuEnvironmentType.Lobby;
        }

        #endregion

        #region Utils

        private static bool ExMachinaVisibleToRole(PlayerRole playerRole) {
            return playerRole.IsAnyAdmin() || playerRole.IsAnyRT() || playerRole.IsAnySupporter();
        }

        private static bool RtToolsVisibleToRole(PlayerRole playerRole) {
            return playerRole.IsAnyAdmin() || playerRole.IsAnyRT();
        }

        #endregion
    }
}
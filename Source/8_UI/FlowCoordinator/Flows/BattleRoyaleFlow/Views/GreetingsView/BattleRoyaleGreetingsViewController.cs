using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleGreetingsViewController : ViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;
        [Inject] private readonly BattleRoyaleReplaySelectionViewController _replaySelectionViewController = null!;
        [Inject] private readonly BattleRoyaleFlowCoordinator _battleRoyaleFlowCoordinator = null!;
        [Inject] private readonly LevelSelectionFlowCoordinator _levelSelectionFlowCoordinator = null!;

        #endregion

        #region Setup

        private void Awake() {
            Construct();
            _battleRoyaleHost.ReplayBeatmapChangedEvent += HandleHostBeatmapChanged;
            _battleRoyaleHost.CanLaunchBattleStateChangedEvent += HandleHostCanLaunchStateChanged;
            _battleRoyaleHost.CanMutateLobbyStateChangedEvent += HandleHostCanMutateStateChanged;
            _battleRoyaleHost.ReplayAddedEvent += HandleHostReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleHostReplayRemoved;
            _battleRoyaleHost.ReplayNavigationRequestedEvent += HandleHostNavigationRequested;
            HandleHostCanLaunchStateChanged(_battleRoyaleHost.CanLaunchBattle);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            _battleRoyaleHost.ReplayBeatmapChangedEvent -= HandleHostBeatmapChanged;
            _battleRoyaleHost.CanLaunchBattleStateChangedEvent -= HandleHostCanLaunchStateChanged;
            _battleRoyaleHost.CanMutateLobbyStateChangedEvent -= HandleHostCanMutateStateChanged;
            _battleRoyaleHost.ReplayAddedEvent -= HandleHostReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleHostReplayRemoved;
            _battleRoyaleHost.ReplayNavigationRequestedEvent -= HandleHostNavigationRequested;
        }

        #endregion

        #region Panel

        private class ClickablePanel : ReactiveComponent {
            public IReactiveComponent? Component {
                get => _component;
                set {
                    if (_component != null) {
                        _backgroundButton.Children.Remove(_component);
                    }
                    _component = value;
                    if (_component != null) {
                        _component.Enabled = ShowComponent;
                        _backgroundButton.Children.Add(_component);
                    }
                }
            }

            public string EmptyText {
                get => _emptyLabel.Text;
                set => _emptyLabel.Text = value;
            }

            public bool ShowComponent {
                get => _showComponent;
                set {
                    _showComponent = value;
                    if (_component == null) return;
                    _component.Enabled = value;
                    _emptyLabel.Enabled = !value;
                }
            }

            public bool Interactable {
                get => _graphicState.IsInteractable;
                set => _graphicState.IsInteractable = value;
            }

            public Action? OnClick;

            private IReactiveComponent? _component;
            private bool _showComponent;

            private State<GraphicState> _graphicState = null!;
            private Background _backgroundButton = null!;
            private Label _emptyLabel = null!;

            protected override GameObject Construct() {
                _graphicState = Remember(GraphicState.None);

                return new Background {
                        FlexController = {
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Stretch
                        },

                        Sprite = BundleLoader.Sprites.background,
                        Material = GameResources.UINoGlowMaterial,
                        PixelsPerUnit = 10f,
                        Skew = BeatSaberStyle.Skew,
                        sColor = _graphicState.MapColorSet(BeatSaberStyle.BsSegmentedControlColors.CellBackgroundColors),
                        Children = {
                            new Label {
                                FlexItem = {
                                    Size = YogaVector.Auto
                                },

                                FontStyle = FontStyles.Italic,
                                Color = BeatSaberStyle.SecondaryTextColor
                            }.Bind(ref _emptyLabel),
                        }
                    }
                    .WithPointerEvents(
                        onEnter: _ => _graphicState.IsHovered = true,
                        onLeave: _ => _graphicState.IsHovered = false,
                        onDown: _ => {
                            if (!_graphicState.IsInteractable) {
                                return;
                            }
                            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                            OnClick?.Invoke();
                        }
                    )
                    .Bind(ref _backgroundButton)
                    .Use();
            }
        }

        #endregion

        #region Construct

        private BeatmapPreviewPanel _beatmapPreviewPanel = null!;
        private ReplaysPreviewPanel _replaysPreviewPanel = null!;
        private ClickablePanel _replaysPanel = null!;
        private ClickablePanel _beatmapPanel = null!;
        private BsPrimaryTextButton _battleButton = null!;

        private void Construct() {
            new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.Center,
                    AlignItems = Align.Center,
                    Gap = 2f
                },

                Children = {
                    new ClickablePanel {
                        FlexItem = {
                            Size = new() { x = 88f, y = 16f }
                        },

                        EmptyText = "NO LEVEL SELECTED",
                        OnClick = PresentLevelSelectionFlow,

                        Component = new BeatmapPreviewPanel {
                            LayoutModifier = new YogaModifier {
                                Size = YogaVector.Auto,
                                FlexGrow = 1f
                            },

                            ShowDifficultyInsteadOfTime = true,
                            Skew = BeatSaberStyle.Skew
                        }.Bind(ref _beatmapPreviewPanel)
                    }.Bind(ref _beatmapPanel),

                    new ClickablePanel {
                        FlexItem = {
                            Size = new() { x = 80f, y = 10f }
                        },

                        EmptyText = "NO REPLAYS",
                        Interactable = false,
                        OnClick = PresentReplaySelectionView,
                        Component = new ReplaysPreviewPanel {
                            FlexItem = {
                                FlexGrow = 1f
                            }
                        }.Bind(ref _replaysPreviewPanel)
                    }.Bind(ref _replaysPanel),

                    new BsPrimaryTextButton {
                        FlexItem = {
                            Size = new() { x = 30f, y = 8f }
                        },

                        Text = "BATTLE",
                        Skew = BeatSaberStyle.Skew,
                        Interactable = false,
                        OnClick = () => _battleRoyaleHost.LaunchBattle()
                    }.Bind(ref _battleButton)
                }
            }.Use(transform);
        }

        private void RefreshReplaysPanel() {
            _replaysPanel.ShowComponent = _replaysPreviewPanel.Replays.Count > 0;
        }

        #endregion

        #region LevelSelectionFlow

        private void PresentLevelSelectionFlow() {
            _levelSelectionFlowCoordinator.AllowDifficultySelection = true;
            _levelSelectionFlowCoordinator.BeatmapSelectedEvent += HandleLevelSelectionFlowBeatmapSelected;
            _levelSelectionFlowCoordinator.FlowCoordinatorDismissedEvent += HandleLevelSelectionFlowDismissed;
            _battleRoyaleFlowCoordinator.PresentFlowCoordinator(_levelSelectionFlowCoordinator);
        }

        private void HandleLevelSelectionFlowBeatmapSelected(BeatmapLevelWithKey beatmap) {
            _battleRoyaleHost.ReplayBeatmap = beatmap;
        }

        #endregion

        #region ReplaySelectionView

        private bool _isInReplayMenu;

        private void PresentReplaySelectionView() {
            _battleRoyaleFlowCoordinator.PushReturnAction("SELECT REPLAY", DismissReplaySelectionView);
            __PresentViewController(_replaySelectionViewController, null);
            _isInReplayMenu = true;
        }

        private void DismissReplaySelectionView() {
            _replaySelectionViewController.__DismissViewController(null);
            _isInReplayMenu = false;
        }

        private void HandleLevelSelectionFlowDismissed() {
            _levelSelectionFlowCoordinator.BeatmapSelectedEvent -= HandleLevelSelectionFlowBeatmapSelected;
            _levelSelectionFlowCoordinator.FlowCoordinatorDismissedEvent -= HandleLevelSelectionFlowDismissed;
        }

        #endregion

        #region Callbacks

        private void HandleHostNavigationRequested(IBattleRoyaleReplay replay) {
            if (!_isInReplayMenu) PresentReplaySelectionView();
        }

        private void HandleHostReplayAdded(IBattleRoyaleReplay replay, object caller) {
            _replaysPreviewPanel.Replays.Add(replay.ReplayHeader);
            RefreshReplaysPanel();
        }

        private void HandleHostReplayRemoved(IBattleRoyaleReplay replay, object caller) {
            _replaysPreviewPanel.Replays.Remove(replay.ReplayHeader);
            RefreshReplaysPanel();
        }

        private void HandleHostBeatmapChanged(BeatmapLevelWithKey beatmap) {
            _replaysPanel.Interactable = beatmap.HasValue;
            _beatmapPanel.ShowComponent = beatmap.HasValue;

            if (beatmap.HasValue) {
                _beatmapPreviewPanel.SetBeatmap(beatmap).RunCatching();
            }
        }

        private void HandleHostCanLaunchStateChanged(bool canLaunch) {
            _battleButton.Interactable = canLaunch;
        }

        private void HandleHostCanMutateStateChanged(bool canMutate) {
            _replaysPanel.Interactable = canMutate && _battleRoyaleHost.ReplayBeatmap.HasValue;
            _beatmapPanel.Interactable = canMutate;
        }

        #endregion
    }
}
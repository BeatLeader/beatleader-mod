using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using HMUI;
using Reactive;
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
            _battleRoyaleHost.ReplayAddedEvent += HandleHostReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleHostReplayRemoved;
            _battleRoyaleHost.ReplayNavigationRequestedEvent += HandleHostNavigationRequested;
            HandleHostCanLaunchStateChanged(_battleRoyaleHost.CanLaunchBattle);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            _battleRoyaleHost.ReplayBeatmapChangedEvent -= HandleHostBeatmapChanged;
            _battleRoyaleHost.CanLaunchBattleStateChangedEvent -= HandleHostCanLaunchStateChanged;
            _battleRoyaleHost.ReplayAddedEvent -= HandleHostReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleHostReplayRemoved;
            _battleRoyaleHost.ReplayNavigationRequestedEvent -= HandleHostNavigationRequested;
        }

        #endregion

        #region Panel

        private class ClickablePanel<T> : ReactiveComponent where T : ILayoutItem, IReactiveComponent {
            public T? Component {
                get => _component;
                set {
                    if (_component != null) {
                        _backgroundButton.Children.Remove(_component);
                    }
                    _component = value;
                    if (_component != null) {
                        _component.WithoutModifier().WithRectExpand();
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
                get => _backgroundButton.Interactable;
                set => _backgroundButton.Interactable = value;
            }

            public Action? OnClick;

            private T? _component;
            private bool _showComponent;

            private ImageButtonLayout _backgroundButton = null!;
            private Label _emptyLabel = null!;

            protected override GameObject Construct() {
                return new ImageButtonLayout {
                        Image = {
                            Sprite = BundleLoader.Sprites.background,
                            Material = GameResources.UINoGlowMaterial,
                            PixelsPerUnit = 10f,
                            Skew = UIStyle.Skew
                        },
                        Colors = UIStyle.ControlColorSet,
                        OnClick = () => OnClick?.Invoke(),
                        Children = {
                            new Label {
                                FontStyle = FontStyles.Italic,
                                Color = UIStyle.SecondaryTextColor
                            }.AsFlexItem(size: "auto").Bind(ref _emptyLabel),
                        }
                    }
                    .AsFlexGroup(alignItems: Align.Center)
                    .Bind(ref _backgroundButton)
                    .Use();
            }
        }

        #endregion

        #region Construct

        private BeatmapPreviewPanel _beatmapPreviewPanel = null!;
        private ReplaysPreviewPanel _replaysPreviewPanel = null!;
        private ClickablePanel<ReplaysPreviewPanel> _replaysPanel = null!;
        private ClickablePanel<BeatmapPreviewPanel> _beatmapPanel = null!;
        private BsPrimaryButton _battleButton = null!;

        private void Construct() {
            new Layout {
                Children = {
                    new ClickablePanel<BeatmapPreviewPanel> {
                            EmptyText = "NO LEVEL SELECTED",
                            OnClick = PresentLevelSelectionFlow,
                            Component = new BeatmapPreviewPanel {
                                ShowDifficultyInsteadOfTime = true,
                                Skew = UIStyle.Skew
                            }.Bind(ref _beatmapPreviewPanel)
                        }
                        .AsFlexItem(size: new() { x = 88f, y = 16f })
                        .Bind(ref _beatmapPanel),
                    //
                    new ClickablePanel<ReplaysPreviewPanel> {
                            EmptyText = "NO REPLAYS",
                            Interactable = false,
                            OnClick = PresentReplaySelectionView,
                            Component = new ReplaysPreviewPanel().Bind(ref _replaysPreviewPanel)
                        }
                        .AsFlexItem(size: new() { x = 80f, y = 10f })
                        .Bind(ref _replaysPanel),
                    //
                    new BsPrimaryButton {
                            Text = "BATTLE",
                            Skew = UIStyle.Skew,
                            Interactable = false,
                            OnClick = () => _battleRoyaleHost.LaunchBattle()
                        }
                        .AsFlexItem(size: new() { x = 30f, y = 8f })
                        .Bind(ref _battleButton)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                alignItems: Align.Center,
                gap: 2f
            ).Use(transform);
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
            _beatmapPreviewPanel.SetBeatmap(beatmap).RunCatching();
        }

        private void HandleHostCanLaunchStateChanged(bool canLaunch) {
            _battleButton.Interactable = canLaunch;
        }

        #endregion
    }
}
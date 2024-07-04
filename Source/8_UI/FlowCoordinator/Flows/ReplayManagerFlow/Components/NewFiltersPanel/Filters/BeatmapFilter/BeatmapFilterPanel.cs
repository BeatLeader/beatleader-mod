using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatSaberMarkupLanguage;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeaderBase> {
        #region Dummies

        private class NotSelectedPreviewBeatmapLevel : IPreviewBeatmapLevel {
            public string? levelID { get; } = string.Empty;
            public string? songName { get; } = "Click to select";
            public string? songSubName { get; } = null;
            public string? songAuthorName { get; } = "Unknown";
            public string? levelAuthorName { get; } = null;

            public float beatsPerMinute { get; } = 0;
            public float songTimeOffset { get; } = -1;
            public float shuffle { get; } = -1;
            public float shufflePeriod { get; } = -1;
            public float previewStartTime { get; } = -1;
            public float previewDuration { get; } = -1;
            public float songDuration { get; } = 0;

            public EnvironmentInfoSO? environmentInfo { get; } = null;
            public EnvironmentInfoSO? allDirectionsEnvironmentInfo { get; } = null;
            public IReadOnlyList<PreviewDifficultyBeatmapSet>? previewDifficultyBeatmapSets { get; } = null;

            public Task<Sprite> GetCoverImageAsync(CancellationToken cancellationToken) => Task.FromResult(BundleLoader.UnknownIcon);
        }

        #endregion

        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeaderBase>>? DependsOn => null;
        public string FilterName => "Beatmap Filter";
        public IPreviewBeatmapLevel? BeatmapLevel { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeaderBase value) {
            var levelId = BeatmapLevel?.levelID;
            return levelId == null || levelId.Replace("custom_level_", "") == value.ReplayInfo.SongHash;
        }

        #endregion

        #region Setup

        public event Action<IPreviewBeatmapLevel?>? BeatmapSelectedEvent;

        private FlowCoordinator? _flowCoordinator;
        private LevelSelectionFlowCoordinator? _selectionFlowCoordinator;

        public void Setup(FlowCoordinator flowCoordinator, LevelSelectionFlowCoordinator selectionFlowCoordinator) {
            _flowCoordinator = flowCoordinator;
            _selectionFlowCoordinator = selectionFlowCoordinator;
        }

        private void Present() {
            if (_flowCoordinator == null || _selectionFlowCoordinator == null) {
                throw new UninitializedComponentException();
            }
            _flowCoordinator.PresentFlowCoordinator(_selectionFlowCoordinator);
            _selectionFlowCoordinator.AllowDifficultySelection = false;
            _selectionFlowCoordinator.FlowCoordinatorDismissedEvent += HandleFlowCoordinatorDismissed;
            _selectionFlowCoordinator.BeatmapSelectedEvent += HandleBeatmapSelected;
        }

        #endregion

        #region Construct

        private BeatmapPreviewPanel _beatmapPreviewPanel = null!;

        private void SetBeatmapLevelPreview(IPreviewBeatmapLevel level) {
            _beatmapPreviewPanel.SetBeatmapLevel(level).ConfigureAwait(true);
        }

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new ImageButton {
                        Image = {
                            Color = Color.white,
                            Sprite = BundleLoader.Sprites.background,
                            PixelsPerUnit = 12f,
                            Material = GameResources.UINoGlowMaterial
                        },
                        Colors = null,
                        GradientColors1 = new StateColorSet {
                            Color = Color.clear,
                            HoveredColor = Color.white.ColorWithAlpha(0.2f)
                        },
                        GrowOnHover = false,
                        HoverLerpMul = float.MaxValue,
                        Skew = UIStyle.Skew,
                        Children = {
                            new BeatmapPreviewPanel {
                                    Skew = UIStyle.Skew
                                }
                                .AsFlexItem(grow: 1f, margin: new() { right = 1f })
                                .Bind(ref _beatmapPreviewPanel)
                        }
                    }.WithClickListener(Present).AsFlexGroup().AsFlexItem(
                        grow: 1f,
                        margin: new() { left = 1f, right = 1f }
                    )
                }
            }.AsFlexGroup(padding: 1f).Use();
        }

        protected override void OnInitialize() {
            SetBeatmapLevelPreview(new NotSelectedPreviewBeatmapLevel());
            this.AsFlexItem(size: new() { x = 52f, y = 12f });
        }

        #endregion

        #region Callbacks

        private void HandleFlowCoordinatorDismissed() {
            _selectionFlowCoordinator!.FlowCoordinatorDismissedEvent -= HandleFlowCoordinatorDismissed;
            _selectionFlowCoordinator.BeatmapSelectedEvent -= HandleBeatmapSelected;
        }

        private void HandleBeatmapSelected(IDifficultyBeatmap beatmap) {
            BeatmapLevel = beatmap.level;
            SetBeatmapLevelPreview(beatmap.level);
            BeatmapSelectedEvent?.Invoke(BeatmapLevel);
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}
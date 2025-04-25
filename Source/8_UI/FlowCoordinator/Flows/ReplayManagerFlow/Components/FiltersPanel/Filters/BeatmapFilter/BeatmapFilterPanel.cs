using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        #region Dummies

        private class NotSelectedPreviewMediaData : IPreviewMediaData {
            public Task<Sprite> GetCoverSpriteAsync() {
                return Task.FromResult(BundleLoader.UnknownIcon);
            }

            public Task<AudioClip?> GetPreviewAudioClip() {
                return Task.FromResult<AudioClip?>(null);
            }

            public void UnloadPreviewAudioClip() { }

            public void UnloadCoverSprite() { }
        }

        private static readonly BeatmapLevel previewBeatmapLevel = new(
            0,
            false,
            "",
            "Click to select",
            null,
            null,
            Array.Empty<string>(),
            Array.Empty<string>(),
            0f,
            0f,
            0f,
            0f,
            0f,
            0f,
            PlayerSensitivityFlag.Safe,
            new NotSelectedPreviewMediaData(),
            null
        );

        #endregion

        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn => null;
        public string FilterName => "Beatmap Filter";
        public string FilterStatus { get; private set; } = null!;
        public BeatmapLevelWithKey BeatmapLevel { get; private set; }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            if (!BeatmapLevel.HasValue) return false;
            var levelId = BeatmapLevel.Level.levelID;
            return levelId.Replace("custom_level_", "") == value.ReplayInfo.SongHash;
        }

        private void RefreshFilterStatus() {
            var level = BeatmapLevel;
            FilterStatus = level.HasValue ? $"Map: {FormatUtils.TruncateEllipsis(level.Level.songName, 15)}" : "No Map";
        }

        #endregion

        #region Setup

        public event Action<BeatmapLevelWithKey>? BeatmapSelectedEvent;

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

        private void SetBeatmapLevel(BeatmapLevel level) {
            _beatmapPreviewPanel.SetBeatmapLevel(level).ConfigureAwait(true);
        }

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new BackgroundButton {
                        Image = {
                            Color = Color.white,
                            Sprite = BundleLoader.Sprites.background,
                            PixelsPerUnit = 12f,
                            Material = GameResources.UINoGlowMaterial
                        },
                        Colors = null,
                        OnClick = Present,
                        GradientColors1 = new SimpleColorSet {
                            Color = Color.clear,
                            HoveredColor = Color.white.ColorWithAlpha(0.2f)
                        },
                        Skew = UIStyle.Skew,
                        Children = {
                            new BeatmapPreviewPanel {
                                    Skew = UIStyle.Skew
                                }
                                .AsFlexItem(flexGrow: 1f, margin: new() { right = 1f })
                                .Bind(ref _beatmapPreviewPanel)
                        }
                    }.AsFlexGroup().AsFlexItem(
                        flexGrow: 1f,
                        margin: new() { left = 1f, right = 1f }
                    )
                }
            }.AsFlexGroup(padding: new() { top = 1f, bottom = 1f }).Use();
        }

        protected override void OnInitialize() {
            RefreshFilterStatus();
            SetBeatmapLevel(previewBeatmapLevel);
            this.AsFlexItem(size: new() { x = 52f, y = 12f });
        }

        #endregion

        #region Callbacks

        private void HandleFlowCoordinatorDismissed() {
            _selectionFlowCoordinator!.FlowCoordinatorDismissedEvent -= HandleFlowCoordinatorDismissed;
            _selectionFlowCoordinator.BeatmapSelectedEvent -= HandleBeatmapSelected;
        }

        private void HandleBeatmapSelected(BeatmapLevelWithKey beatmap) {
            BeatmapLevel = beatmap;
            RefreshFilterStatus();
            SetBeatmapLevel(beatmap.Level);
            BeatmapSelectedEvent?.Invoke(BeatmapLevel);
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}
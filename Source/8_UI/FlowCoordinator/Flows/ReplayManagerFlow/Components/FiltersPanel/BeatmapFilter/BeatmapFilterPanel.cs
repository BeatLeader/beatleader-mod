using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterPanel : ReactiveComponent, IPanelListFilter<IReplayHeader> {
        #region Filter

        public IEnumerable<IPanelListFilter<IReplayHeader>>? DependsOn => null;
        public string FilterName => "Map";
        public bool IsActive => BeatmapLevel.HasValue;

        public BeatmapLevelWithKey BeatmapLevel {
            get => _beatmapLevel.Value;
            private set => _beatmapLevel.Value = value;
        }

        public void Reset() {
            if (!IsActive) return;
            BeatmapLevel = default;
            OnBeatmapSelected?.Invoke(BeatmapLevel);
            FilterUpdatedEvent?.Invoke();
        }

        public event Action? FilterUpdatedEvent;

        public bool Matches(IReplayHeader value) {
            if (!BeatmapLevel.HasValue) return true;
            var levelId = BeatmapLevel.Level.levelID;
            return levelId.Replace("custom_level_", "") == value.ReplayInfo.SongHash;
        }

        #endregion

        #region Setup

        public Action<BeatmapLevelWithKey>? OnBeatmapSelected { get; set; }

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
            _selectionFlowCoordinator.AllowDifficultySelection = false;
            _flowCoordinator.PresentFlowCoordinator(_selectionFlowCoordinator);
            _selectionFlowCoordinator.FlowCoordinatorDismissedEvent += HandleFlowCoordinatorDismissed;
            _selectionFlowCoordinator.BeatmapSelectedEvent += HandleBeatmapSelected;
        }

        #endregion

        #region Construct

        private State<BeatmapLevelWithKey> _beatmapLevel = null!;
        private State<Sprite> _cover = null!;

        protected override GameObject Construct() {
            var selectorState = Remember(GraphicState.None);
            var clearState = Remember(GraphicState.None);

            _cover = Remember(BundleLoader.UnknownIcon);
            _beatmapLevel = Remember<BeatmapLevelWithKey>(default);
            _beatmapLevel.AddCallback(x => _ = LoadCoverAsync(x).RunCatching(), lazy: true);

            return new Layout {
                FlexController = {
                    Gap = 0.5f.pt
                },

                FlexItem = {
                    Size = new() { y = 8f }
                },

                Children = {
                    new Background {
                        FlexController = {
                            AlignItems = Align.Center,
                            Gap = 1.5f,
                            Padding = new() { right = 1f }
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        PixelsPerUnit = 10f,
                        
                        sSprite = _beatmapLevel.Map(x => x.HasValue ?
                            BeatSaberResources.Sprites.backgroundLeft :
                            BeatSaberResources.Sprites.background
                        ),
                        
                        sColor = selectorState.MapColorSet(BeatSaberStyle.BsSegmentedControlColors.CellBackgroundColors),

                        Children = {
                            new Background {
                                FlexItem = {
                                    Size = new() { y = 100.pct },
                                    AspectRatio = 1f
                                },

                                Sprite = BeatSaberResources.Sprites.backgroundLeft,
                                PixelsPerUnit = 10f,

                                Children = {
                                    new Image {
                                        sSprite = _cover
                                    }.WithRectExpand()
                                }
                            }.With(static x => x.Content.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false),

                            new Layout {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column
                                },

                                FlexItem = {
                                    FlexGrow = 1f,
                                    FlexShrink = 1f,
                                    AlignSelf = Align.Stretch
                                },

                                Children = {
                                    new Label {
                                        FlexItem = {
                                            PositionType = PositionType.Absolute,
                                            Position = new() { bottom = 30.pct, left = 0f, right = 0f }
                                        },

                                        FontSize = 3.5f,
                                        Alignment = TextAlignmentOptions.BottomLeft,
                                        EnableWrapping = false,
                                        Overflow = TextOverflowModes.Ellipsis,

                                        sText = _beatmapLevel.Map(static x => x.HasValue ? x.Level.songName : "Click to select a map")
                                    },

                                    new Label {
                                        FlexItem = {
                                            PositionType = PositionType.Absolute,
                                            Position = new() { top = 50.pct, left = 0f, right = 0f }
                                        },

                                        FontSize = 2.8f,
                                        Alignment = TextAlignmentOptions.TopLeft,
                                        EnableWrapping = false,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        Color = BeatSaberStyle.SecondaryTextColor,

                                        sText = _beatmapLevel.Map(static x => x.HasValue ? x.Level.songAuthorName : "Any map")
                                    }
                                }
                            }
                        }
                    }.WithPointerEvents(
                        onEnter: _ => selectorState.IsHovered = true,
                        onLeave: _ => selectorState.IsHovered = false,
                        onDown: _ => {
                            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                            Present();
                        }
                    ),

                    new Background {
                        FlexController = {
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Center
                        },

                        FlexItem = {
                            Size = new() { x = 5.pt }
                        },

                        Sprite = BeatSaberResources.Sprites.backgroundRight,
                        PixelsPerUnit = 10f,
                        sColor = clearState.MapColorSet(BeatSaberStyle.BsSegmentedControlColors.CellBackgroundColors),
                        sEnabled = _beatmapLevel.Map(static x => x.HasValue),

                        Children = {
                            new Image {
                                FlexItem = {
                                    Size = 2f
                                },

                                Sprite = global::Reactive.BeatSaber.GameResources.CloseIcon,
                                PreserveAspect = true,
                                sColor = clearState.MapColorSet(BeatSaberStyle.BsButtonColors.ContentColors)
                            }
                        }
                    }.WithPointerEvents(
                        onEnter: _ => clearState.IsHovered = true,
                        onLeave: _ => clearState.IsHovered = false,
                        onDown: _ => {
                            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                            Reset();
                        }
                    )
                }
            }.Use();
        }

        private async Task LoadCoverAsync(BeatmapLevelWithKey beatmap) {
            var cover = beatmap.HasValue ? await beatmap.Level.previewMediaData.GetCoverSpriteAsync() : BundleLoader.UnknownIcon;

            if (beatmap.Level == _beatmapLevel.Value.Level) {
                _cover.Value = cover;
            }
        }

        #endregion

        #region Callbacks

        private void HandleFlowCoordinatorDismissed() {
            _selectionFlowCoordinator!.FlowCoordinatorDismissedEvent -= HandleFlowCoordinatorDismissed;
            _selectionFlowCoordinator.BeatmapSelectedEvent -= HandleBeatmapSelected;
        }

        private void HandleBeatmapSelected(BeatmapLevelWithKey beatmap) {
            BeatmapLevel = beatmap;
            OnBeatmapSelected?.Invoke(BeatmapLevel);
            FilterUpdatedEvent?.Invoke();
        }

        #endregion
    }
}
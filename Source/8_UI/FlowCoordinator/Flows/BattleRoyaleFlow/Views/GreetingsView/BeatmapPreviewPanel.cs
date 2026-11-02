using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapPreviewPanel : ReactiveComponent, ISkewedComponent {
        #region Skew

        public float Skew {
            get => _skew;
            set {
                _skew = value;
                var style = value > 0 ? FontStyles.Italic : FontStyles.Normal;
                _songNameLabel.FontStyle = style;
                _songAuthorLabel.FontStyle = style;
                _songTimeLabel.FontStyle = style;
                _songBpmLabel.FontStyle = style;
                _songDifficultyLabel.FontStyle = style;
                _songImage.Skew = value;
                _songDifficultyImage.Skew = value;
            }
        }

        private float _skew;

        #endregion

        #region Setup

        public bool ShowDifficultyInsteadOfTime {
            set {
                _songDifficultyContainer.Enabled = value;
                _songTimeLabel.Enabled = !value;
                _songBpmLabel.Enabled = !value;
            }
        }

        public async Task SetBeatmap(BeatmapLevelWithKey beatmap) {
            await SetBeatmapLevel(beatmap.Level);
            _songDifficultyLabel.Text = beatmap.Key.difficulty.ToString();
            _songDifficultyImage.Sprite = beatmap.Key.beatmapCharacteristic.icon;
        }

        public async Task SetBeatmapLevel(BeatmapLevel level) {
            _songNameLabel.Text = FormatSongNameText(level.songName, level.songSubName);

            var mappers = string.Join(", ", level.allMappers);
            _songAuthorLabel.Text = FormatAuthorText(level.songAuthorName, mappers);
            
            _songTimeLabel.Text = FormatUtils.FormatTime(level.songDuration);
            _songBpmLabel.Text = Mathf.FloorToInt(level.beatsPerMinute).ToString();
            _songImage.Sprite = await level.previewMediaData.GetCoverSpriteAsync();
        }

        private static string FormatSongNameText(string name, string subName) {
            return $"{name} <size=80%>{subName}</size>";
        }

        private static string FormatAuthorText(string author, string mapper) {
            var text = $"<size=80%>{author}</size>";
            
            if (!string.IsNullOrEmpty(mapper)) {
                text += $" <size=90%>[<color=#89ff89>{mapper}</color>]</size>";
            }
            
            return text;
        }

        #endregion

        #region Construct

        private Layout _songDifficultyContainer = null!;
        private Label _songDifficultyLabel = null!;
        private Image _songDifficultyImage = null!;

        private Label _songNameLabel = null!;
        private Label _songAuthorLabel = null!;
        private Label _songTimeLabel = null!;
        private Label _songBpmLabel = null!;

        private Layout _background = null!;
        private Image _songImage = null!;

        protected override GameObject Construct() {
            static Label Label(
                TextOverflowModes overflow,
                TextAlignmentOptions alignment,
                float minFontSize,
                float maxFontSize,
                float size,
                Color color,
                ref Label variable
            ) {
                return new Label {
                    FlexItem = {
                        FlexGrow = size
                    },

                    Overflow = overflow,
                    Alignment = alignment,
                    FontStyle = FontStyles.Italic,
                    FontSizeMin = minFontSize,
                    FontSizeMax = maxFontSize,
                    EnableAutoSizing = true,
                    Color = color
                }.Bind(ref variable);
            }

            var primaryColor = Color.white;
            var secondaryColor = Color.white.ColorWithAlpha(0.75f);

            return new Layout {
                FlexController = {
                    Gap = 0.8f
                },

                Children = {
                    new Image {
                        FlexItem = {
                            AspectRatio = 1f
                        },

                        Sprite = BundleLoader.UnknownIcon,
                        Material = GameResources.UINoGlowRoundEdgeMaterial
                    }.Bind(ref _songImage),
                    //
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center
                        },

                        FlexItem = {
                            FlexGrow = 1f,
                            MinSize = new() { y = 8f },
                            MaxSize = new() { y = "100%" },
                            Size = new() { y = 12f },
                            Margin = new() { right = 2f },
                            AlignSelf = Align.Center
                        },

                        Children = {
                            //top rail
                            new Layout {
                                FlexItem = {
                                    Position = new() { top = 0f },
                                    PositionType = PositionType.Absolute,
                                    Size = new() { x = 100.pct, y = 70.pct },
                                    Margin = new() { left = 0.7f, right = 1.5f }
                                },

                                Children = {
                                    //song name
                                    Label(
                                        TextOverflowModes.Ellipsis,
                                        TextAlignmentOptions.BottomLeft,
                                        4f,
                                        5f,
                                        8f,
                                        primaryColor,
                                        ref _songNameLabel
                                    ),
                                    //song time
                                    Label(
                                        TextOverflowModes.Overflow,
                                        TextAlignmentOptions.BottomRight,
                                        4f,
                                        5f,
                                        2f,
                                        primaryColor,
                                        ref _songTimeLabel
                                    )
                                }
                            }.AsFlexGroup(),
                            //bottom rail
                            new Layout {
                                FlexItem = {
                                    Position = new() { bottom = 0f },
                                    PositionType = PositionType.Absolute,
                                    Size = new() { x = 100.pct, y = 50.pct },
                                    Margin = new() { right = 2.5f }
                                },

                                Children = {
                                    //song author
                                    Label(
                                        TextOverflowModes.Ellipsis,
                                        TextAlignmentOptions.TopLeft,
                                        3f,
                                        4f,
                                        7f,
                                        secondaryColor,
                                        ref _songAuthorLabel
                                    ),
                                    //song bpm
                                    Label(
                                        TextOverflowModes.Overflow,
                                        TextAlignmentOptions.TopRight,
                                        3f,
                                        4f,
                                        3f,
                                        secondaryColor,
                                        ref _songBpmLabel
                                    )
                                }
                            }.AsFlexGroup(),
                            //difficulty
                            new Layout {
                                FlexController = {
                                    AlignItems = Align.Center,
                                    Gap = 1f
                                },

                                FlexItem = {
                                    Size = new() { y = "100%" },
                                    Position = new() { right = 0f },
                                    PositionType = PositionType.Absolute
                                },

                                Children = {
                                    new Label {
                                        FlexItem = {
                                            Size = "auto"
                                        },

                                        FontSize = 4f,
                                        Color = secondaryColor
                                    }.Bind(ref _songDifficultyLabel),
                                    //
                                    new Image {
                                        FlexItem = {
                                            Size = 4f
                                        },

                                        Sprite = BundleLoader.Sprites.transparentPixel,
                                        Color = secondaryColor,
                                        PreserveAspect = true
                                    }.Bind(ref _songDifficultyImage)
                                }
                            }.Bind(ref _songDifficultyContainer)
                            //
                        }
                    }
                }
            }.Bind(ref _background).Use();
        }

        protected override void OnInitialize() {
            this.FlexItem.Size = new() { x = 50f, y = 10f };
            ShowDifficultyInsteadOfTime = false;
        }

        #endregion
    }
}
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
                    Overflow = overflow,
                    Alignment = alignment,
                    FontStyle = FontStyles.Italic,
                    FontSizeMin = minFontSize,
                    FontSizeMax = maxFontSize,
                    EnableAutoSizing = true,
                    Color = color
                }.AsFlexItem(grow: size).Bind(ref variable);
            }

            var primaryColor = Color.white;
            var secondaryColor = Color.white.ColorWithAlpha(0.75f);

            return new Layout {
                Children = {
                    new Image {
                        Sprite = BundleLoader.UnknownIcon,
                        Material = GameResources.UINoGlowRoundEdgeMaterial
                    }.AsFlexItem(aspectRatio: 1f).Bind(ref _songImage),
                    //
                    new Layout {
                        Children = {
                            //top rail
                            new Layout {
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
                            }.AsFlexGroup().AsFlexItem(
                                position: new() { top = 0f },
                                size: new() { x = "100%", y = "60%" },
                                margin: new() { left = 0.7f, right = 1.5f }
                            ),
                            //bottom rail
                            new Layout {
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
                            }.AsFlexGroup().AsFlexItem(
                                position: new() { bottom = 0f },
                                size: new() { x = "100%", y = "60%" },
                                margin: new() { right = 2.5f }
                            ),
                            //difficulty
                            new Layout {
                                Children = {
                                    new Label {
                                        FontSize = 4f,
                                        Color = secondaryColor
                                    }.AsFlexItem(size: "auto").Bind(ref _songDifficultyLabel),
                                    //
                                    new Image {
                                        Sprite = BundleLoader.Sprites.transparentPixel,
                                        Color = secondaryColor,
                                        PreserveAspect = true
                                    }.AsFlexItem(size: 4f).Bind(ref _songDifficultyImage)
                                }
                            }.AsFlexGroup(
                                alignItems: Align.Center,
                                gap: 1f
                            ).AsFlexItem(
                                size: new() { y = "100%" },
                                position: new() { right = 0f }
                            ).Bind(ref _songDifficultyContainer)
                            //
                        }
                    }.AsFlexGroup(
                        direction: FlexDirection.Column,
                        justifyContent: Justify.Center
                    ).AsFlexItem(
                        grow: 1f,
                        minSize: new() { y = 8f },
                        maxSize: new() { y = "100%" },
                        size: new() { y = 12f },
                        margin: new() { right = 2f },
                        alignSelf: Align.Center
                    )
                }
            }.AsFlexGroup(gap: 0.8f).Bind(ref _background).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 50f, y = 10f });
            ShowDifficultyInsteadOfTime = false;
        }

        #endregion
    }
}
using System.Linq;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class BeatmapLevelPreview : ReactiveComponent {
        #region Construct

        private Image _songPreviewImage = null!;
        private Label _songNameLabel = null!;
        private Label _songAuthorLabel = null!;

        protected override GameObject Construct() {
            static ReactiveComponent CreateLabel(ref Label label) {
                return new Background {
                    FlexController = {
                        Padding = new() { left = 1f, right = 1f },
                        Gap = new() { x = 1f },
                        Overflow = Overflow.Hidden
                    },

                    FlexItem = {
                        Size = "auto",
                        MaxSize = "100%"
                    },

                    Children = {
                        new Label {
                            FlexItem = {
                                FlexShrink = 1f,
                                Size = "auto"
                            },

                            Overflow = TextOverflowModes.Ellipsis,
                            FontSize = 4f
                        }.Bind(ref label)
                    }
                }.AsBackground(
                    material: BundleLoader.Materials.tintedBlurredBackgroundMaterial,
                    pixelsPerUnit: 12f
                );
            }

            static ReactiveComponent CreateRail(ILayoutItem child) {
                return new Layout {
                    FlexController = {
                        JustifyContent = Justify.FlexStart,
                        Gap = new() { x = 0.5f }
                    },

                    Children = {
                        child
                    }
                };
            }

            return new Layout {
                FlexController = {
                    Padding = new() {
                        top = 2f,
                        bottom = 2f,
                        left = 1.5f,
                        right = 1.5f
                    },
                    Gap = 2f
                },

                Children = {
                    //song image
                    new Image {
                        FlexItem = {
                            AspectRatio = 1f
                        },

                        Material = BundleLoader.RoundTextureMaterial
                    }.Bind(ref _songPreviewImage),
                    //labels
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Stretch,
                            Gap = new() { y = 1f }
                        },

                        FlexItem = {
                            FlexGrow = 1f,
                            FlexShrink = 1f
                        },

                        Children = {
                            //song name
                            CreateRail(
                                CreateLabel(ref _songNameLabel)
                            ),
                            //song author
                            CreateRail(
                                CreateLabel(ref _songAuthorLabel)
                            )
                        }
                    }
                }
            }.Use();
        }

        #endregion

        #region Setup

        public void SetBeatmapLevel(BeatmapLevel level) {
            _songNameLabel.Text = level.songName;
            if (level.allMappers is { Length: > 0 }) {
                _songAuthorLabel.Text = string.Join(", ", level.allMappers);
            }
            LoadAndAssignImage(level);
        }

        private async void LoadAndAssignImage(BeatmapLevel level) {
            _songPreviewImage.Sprite = await level.previewMediaData.GetCoverSpriteAsync();
        }

        #endregion
    }
}
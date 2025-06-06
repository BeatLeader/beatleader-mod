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
                    Children = {
                        new Label {
                            Overflow = TextOverflowModes.Ellipsis,
                            FontSize = 4f
                        }.AsFlexItem(size: "auto").Bind(ref label)
                    }
                }.AsBackground(
                    material: BundleLoader.Materials.tintedBlurredBackgroundMaterial,
                    pixelsPerUnit: 12f
                ).AsFlexGroup(
                    padding: new() { left = 1f, right = 1f },
                    gap: new() { x = 1f },
                    overflow: Overflow.Hidden
                ).AsFlexItem(
                    size: "auto",
                    maxSize: "100%"
                );
            }

            static ReactiveComponent CreateRail(ILayoutItem child) {
                return new Layout {
                    Children = {
                        child
                    }
                }.AsFlexGroup(
                    justifyContent: Justify.FlexStart,
                    gap: new() { x = 0.5f }
                ).AsFlexItem();
            }

            return new Layout {
                Children = {
                    //song image
                    new Image {
                        Material = BundleLoader.RoundTextureMaterial
                    }.AsFlexItem(aspectRatio: 1f).Bind(ref _songPreviewImage),
                    //labels
                    new Layout {
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
                    }.AsFlexGroup(
                        direction: FlexDirection.Column,
                        justifyContent: Justify.Center,
                        alignItems: Align.FlexStart,
                        gap: new() { y = 1f }
                    ).AsFlexItem(flexGrow: 1f)
                }
            }.AsFlexGroup(padding: new() { top = 2f, bottom = 2f }, gap: 2f).Use();
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
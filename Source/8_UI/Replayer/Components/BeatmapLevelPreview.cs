using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;
using FlexDirection = BeatLeader.UI.Reactive.Yoga.FlexDirection;

namespace BeatLeader.UI.Replayer {
    internal class BeatmapLevelPreview : ReactiveComponent {
        #region Construct

        private Image _songPreviewImage = null!;
        private Label _songNameLabel = null!;
        private Label _songAuthorLabel = null!;

        protected override GameObject Construct() {
            static ReactiveComponentBase CreateLabel(ref Label label) {
                return new Image {
                    Children = {
                        new Label {
                            Overflow = TextOverflowModes.Ellipsis,
                            FontSize = 4f
                        }.AsFlexItem(size: "auto").Bind(ref label)
                    }
                }.AsBlurBackground().AsFlexGroup(
                    padding: new() { left = 1f, right = 1f },
                    gap: new() { x = 1f },
                    overflow: Overflow.Hidden
                ).AsFlexItem(
                    size: "auto",
                    maxSize: "100%"
                );
            }

            static ReactiveComponentBase CreateRail(ILayoutItem child) {
                return new Dummy {
                    Children = {
                        child
                    }
                }.AsFlexGroup(
                    justifyContent: Justify.FlexStart,
                    gap: new() { x = 0.5f }
                ).AsFlexItem();
            }

            return new Dummy {
                Children = {
                    //song image
                    new Image {
                        Material = BundleLoader.RoundTextureMaterial
                    }.AsFlexItem(aspectRatio: 1f).Bind(ref _songPreviewImage),
                    //labels
                    new Dummy {
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
                        gap: new() { y = 1f },
                        independentLayout: true
                    ).AsFlexItem(grow: 1f)
                }
            }.AsFlexGroup(padding: 1f, gap: 2f).Use();
        }

        #endregion

        #region Setup

        public void SetBeatmapLevel(IPreviewBeatmapLevel level) {
            _songNameLabel.Text = level.songName;
            _songAuthorLabel.Text = level.levelAuthorName;
            LoadAndAssignImage(level);
        }

        private async void LoadAndAssignImage(IPreviewBeatmapLevel level) {
            _songPreviewImage.Sprite = await level.GetCoverImageAsync(default);
        }

        #endregion
    }
}
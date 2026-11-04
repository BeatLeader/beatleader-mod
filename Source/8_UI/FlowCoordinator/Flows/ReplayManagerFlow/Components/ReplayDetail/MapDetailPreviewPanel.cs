using System.Net;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models.BeatSaver;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class MapDetailPreviewPanel : ReactiveComponent {
        #region Setup

        public async Task SetData(MapDetail? mapDetail) {
            _authorLabel.Text = $"By {mapDetail?.metadata!.levelAuthorName ?? "Unknown"}";
            _bsrLabel.Text = $"BSR: {mapDetail?.id ?? "Unknown"}";
            //loading cover
            var url = mapDetail?.versions![0].coverURL;
            if (mapDetail != null && url != null) {
                await RefreshMapCoverAsync(url);
            } else {
                _cover.Sprite = BundleLoader.UnknownIcon;
            }
        }

        #endregion

        #region Cover

        private async Task RefreshMapCoverAsync(string url) {
            var task = RawDataRequest.Send(url);
            _cover.Sprite = BundleLoader.UnknownIcon;
            _coverContainer.Loading = true;
            await task.Join();
            //loading sprite
            var sprite = task.RequestStatusCode is HttpStatusCode.OK ? SpriteUtils.CreateSprite(task.Result!) : null;
            _cover.Sprite = sprite ?? BundleLoader.FileError;
            _coverContainer.Loading = false;
        }

        #endregion

        #region Construct

        private LoadingContainer _coverContainer = null!;
        private Image _cover = null!;
        private Label _authorLabel = null!;
        private Label _bsrLabel = null!;

        protected override GameObject Construct() {
            return new Background {
                FlexController = {
                    Padding = 1f,
                    Gap = 1f
                },

                FlexItem = {
                    Size = new() { y = 12f }
                },

                Children = {
                    //cover
                    new Image {
                            FlexItem = {
                                AspectRatio = 1f
                            },

                            Material = GameResources.UINoGlowRoundEdgeMaterial
                        }
                        .Bind(ref _cover)
                        .InLoadingContainer()
                        .AsFlexItem(aspectRatio: 1f)
                        .Bind(ref _coverContainer),
                    //labels
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Children = {
                            //author label
                            new Label {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                Alignment = TextAlignmentOptions.MidlineLeft
                            }.Bind(ref _authorLabel),
                            //bsr label
                            new Label {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                Alignment = TextAlignmentOptions.MidlineLeft
                            }.Bind(ref _bsrLabel)
                        }
                    }
                }
            }.AsBlurBackground(
                color: Color.white.ColorWithAlpha(0.9f)
            ).Use();
        }

        #endregion
    }
}
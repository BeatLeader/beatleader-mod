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
            var task = RawDataRequest.SendRequest(url);
            _cover.Sprite = BundleLoader.UnknownIcon;
            _coverContainer.Loading = true;
            await task.Join();
            //loading sprite
            var sprite = task.RequestStatusCode is HttpStatusCode.OK ? ReactiveUtils.CreateSprite(task.Result!) : null;
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
            return new Image {
                Children = {
                    //cover
                    new Image {
                            Material = GameResources.UINoGlowRoundEdgeMaterial
                        }
                        .AsFlexItem(aspectRatio: 1f)
                        .Bind(ref _cover)
                        .InLoadingContainer()
                        .Bind(ref _coverContainer),
                    //labels
                    new Dummy {
                        Children = {
                            //author label
                            new Label {
                                Alignment = TextAlignmentOptions.MidlineLeft
                            }.AsFlexItem(grow: 1f).Bind(ref _authorLabel),
                            //bsr label
                            new Label {
                                Alignment = TextAlignmentOptions.MidlineLeft
                            }.AsFlexItem(grow: 1f).Bind(ref _bsrLabel)
                        }
                    }.AsFlexGroup(direction: FlexDirection.Column).AsFlexItem(grow: 1f)
                }
            }.AsBlurBackground(
                color: Color.white.ColorWithAlpha(0.9f)
            ).AsFlexGroup(padding: 1f, gap: 1f).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { y = 12f });
        }

        #endregion
    }
}
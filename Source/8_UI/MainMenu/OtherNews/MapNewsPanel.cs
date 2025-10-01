using System.Collections.Generic;
using System.Linq;
using BeatLeader.API;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using static BeatLeader.WebRequests.RequestState;

#pragma warning disable CS0618

namespace BeatLeader.UI.MainMenu {
    internal class MapNewsPanel : ReactiveComponent {
        #region Request

        protected override void OnInitialize() {
            TrendingMapsRequest.Send();
            TrendingMapsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDestroy() {
            TrendingMapsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<TrendingMapData>> instance, WebRequests.RequestState state, string? failReason) {
            if (state == Finished) {
                var list = instance.Result!.data?
                    .Where(m => m.difficulty?.status != 5)
                    .ToList() ?? new List<TrendingMapData>();

                _newsPanel.UpdateFromRequest(state, list);
            } else {
                _newsPanel.UpdateFromRequest(state, new());
            }
        }

        #endregion

        #region Construct

        private NewsPanel<TrendingMapData, MapPreviewPanel> _newsPanel = null!;

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new NewsHeader {
                            Text = "Trending Maps"
                        }.AsFlexItem(),

                        new NewsPanel<TrendingMapData, MapPreviewPanel> {
                            EmptyMessage = "No trending maps",
                            OnCellConstructed = cell => {
                                cell.ButtonAction = item => MapDownloadDialog.OpenSongOrDownloadDialog(item.song, ContentTransform);
                                cell.BackgroundAction = item => MapPreviewDialog.OpenSongOrDownloadDialog(item, ContentTransform);
                            }
                        }.AsFlexItem(flexGrow: 1f).Bind(ref _newsPanel)
                    }
                }
                .AsFlexGroup(
                    padding: 1f,
                    direction: FlexDirection.Column
                )
                .AsBackground(
                    color: Color.black.ColorWithAlpha(0.33f),
                    pixelsPerUnit: 7f
                )
                .AsFlexItem(size: new() { x = 70, y = 39 })
                .Use();
        }

        #endregion
    }
}
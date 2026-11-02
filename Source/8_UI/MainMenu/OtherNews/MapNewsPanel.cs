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
                _newsPanel.UpdateFromRequest(state, null);
            }
        }

        #endregion

        #region Construct

        private NewsPanel<TrendingMapData> _newsPanel = null!;

        protected override GameObject Construct() {
            return new Background {
                    FlexController = {
                        Padding = 0.5f,
                        FlexDirection = FlexDirection.Column
                    },

                    FlexItem = {
                        Size = new() { x = 70 },
                        FlexGrow = 1f
                    },

                    Children = {
                        new NewsHeader {
                            Text = "Trending Maps"
                        },

                        new NewsPanel<TrendingMapData> {
                            FlexItem = {
                                FlexGrow = 1f
                            },

                            EmptyMessage = "No trending maps",
                            ConstructCell = ctx => new MapPreviewPanel(ctx) {
                                ButtonAction = item => MapDownloadDialog.OpenSongOrDownloadDialog(item.song, ContentTransform),
                                BackgroundAction = item => MapPreviewDialog.OpenSongOrDownloadDialog(item, ContentTransform)
                            }
                        }.Bind(ref _newsPanel)
                    }
                }
                .AsBackground(
                    color: Color.black.ColorWithAlpha(0.33f),
                    pixelsPerUnit: 7f
                )
                .Use();
        }

        #endregion
    }
}
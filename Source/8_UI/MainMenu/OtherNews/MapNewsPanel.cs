using System.Collections.Generic;
using System.Linq;
using BeatLeader.API;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class MapNewsPanel : ReactiveComponent {
        private ObservableValue<WebRequests.RequestState> _requestState = null!;
        private ObservableValue<List<TrendingMapData>> _mapList = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            TrendingMapsRequest.Send();
            TrendingMapsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            TrendingMapsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<TrendingMapData>> instance, WebRequests.RequestState state, string? failReason) {
            _requestState.Value = state;
            if (state == WebRequests.RequestState.Finished) {
                _mapList.Value = instance.Result.data?.Where(m => m.difficulty?.status != 5).ToList() ?? new List<TrendingMapData>();
            } else if (state == WebRequests.RequestState.Failed) {
                _mapList.Value = new List<TrendingMapData>();
            }
        }

        protected override GameObject Construct() {
            _requestState = Remember(WebRequests.RequestState.Uninitialized);
            _mapList = Remember(new List<TrendingMapData>());

            return new Background() {
                Children = {
                    new NewsHeader {
                        Text = "Trending Maps"
                    }
                    .AsFlexItem(),

                    new Layout {
                        Children = {
                            new ScrollArea {
                                ScrollContent = new Layout {
                                    Children = {
                                        new Spinner()
                                            .AsFlexItem(
                                                size: new () { x = 8, y = 8 }, 
                                                alignSelf: Align.Center
                                            )
                                            .Animate(_requestState, (spinner, state) => spinner.Enabled = state == WebRequests.RequestState.Started || state == WebRequests.RequestState.Uninitialized),
                                        new Label()
                                            .AsFlexItem(alignSelf: Align.Center)
                                            .Animate(_mapList, (label, list) => {
                                                if (_requestState.Value == WebRequests.RequestState.Failed) {
                                                    label.Text = "<color=#ff8888>Failed to load";
                                                    label.Enabled = true;
                                                } else if (_requestState.Value == WebRequests.RequestState.Finished && list.Count == 0) {
                                                    label.Text = "There are no trending maps";
                                                    label.Enabled = true;
                                                } else {
                                                    label.Enabled = false;
                                                }
                                            }),
                                        new ListView<TrendingMapData, MapPreviewPanel>()
                                        {
                                            WhenCellConstructed = (cell) => {
                                                cell.ButtonAction = (item) => MapDownloadDialog.OpenSongOrDownloadDialog(item.song, Content.transform);
                                                cell.BackgroundAction = (item) => MapPreviewDialog.OpenSongOrDownloadDialog(item, Content.transform);
                                            }
                                        }
                                        .AsFlexGroup(padding: 1f)
                                        .AsFlexItem()
                                        .Animate(_mapList, (listView, list) => {
                                            listView.Items = list;
                                            listView.Enabled = list.Count() > 0 && _requestState.Value == WebRequests.RequestState.Finished;
                                        })
                                    }
                                }
                                .AsFlexGroup(
                                    direction: FlexDirection.Column, 
                                    gap: 1f, 
                                    constrainVertical: false
                                )
                                .AsFlexItem()
                            }
                            .AsFlexItem(flexGrow: 1)
                            .Export(out var scrollArea),
                    
                            new Scrollbar()
                            .AsFlexItem()
                            .With(x => scrollArea.Scrollbar = x),
                        }
                    }
                    .AsFlexGroup()
                    .AsFlexItem(size: new() { y = 33 })
                }
            }
            .AsFlexGroup(
                padding: 1f,
                direction: FlexDirection.Column
            ).AsBackground(
                color: Color.black.ColorWithAlpha(0.33f),
                pixelsPerUnit: 7f
            )
            .AsFlexItem(
                size: new() { x = 70, y = 39 }
            )
            .Use();
        }
    }
} 
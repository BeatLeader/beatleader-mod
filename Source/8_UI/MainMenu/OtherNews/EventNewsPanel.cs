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
    internal class EventNewsPanel : ReactiveComponent {
        private ObservableValue<WebRequests.RequestState> _requestState = null!;
        private ObservableValue<List<PlatformEvent>> _eventList = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            PlatformEventsRequest.Send();
            PlatformEventsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            PlatformEventsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<PlatformEvent>> instance, WebRequests.RequestState state, string? failReason) {
            _requestState.Value = state;
            if (state == WebRequests.RequestState.Finished) {
                _eventList.Value = instance.Result.data ?? new List<PlatformEvent>();
            } else if (state == WebRequests.RequestState.Failed) {
                _eventList.Value = new List<PlatformEvent>();
            }
        }

        protected override GameObject Construct() {
            _requestState = Remember(WebRequests.RequestState.Uninitialized);
            _eventList = Remember(new List<PlatformEvent>());

            return new Background() {
                Children = {
                    new NewsHeader {
                        Text = "BeatLeader Events"
                    }.AsFlexItem(),
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
                                        .Animate(_eventList, (label, list) => {
                                            if (_requestState.Value == WebRequests.RequestState.Failed) {
                                                label.Text = "<color=#ff8888>Failed to load";
                                                label.Enabled = true;
                                            } else if (_requestState.Value == WebRequests.RequestState.Finished && !list.Any()) {
                                                label.Text = "There are no events";
                                                label.Enabled = true;
                                            } else {
                                                label.Enabled = false;
                                            }
                                        }),
                                    new ListView<PlatformEvent, EventPreviewPanel>() {
                                        CellConstructedCb = (cell) => {
                                            cell.ButtonAction = (item) => ReeModalSystem.OpenModal<EventDetailsDialog>(Content.transform, item);
                                            cell.BackgroundAction = (item) => ReeModalSystem.OpenModal<EventDetailsDialog>(Content.transform, item);
                                        }
                                    }
                                    .AsFlexGroup(padding: 1f)
                                    .AsFlexItem()
                                    .Animate(_eventList, (listView, list) => {
                                        listView.Items = list;
                                        listView.Enabled = list.Any() && _requestState.Value == WebRequests.RequestState.Finished;
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
                    .AsFlexItem(size: new() { y = 24 })
                }
            }
            .AsFlexGroup(
                padding: 1f,
                direction: FlexDirection.Column
            ).AsBackground(
                color: Color.black.ColorWithAlpha(0.33f),
                pixelsPerUnit: 7f
            ).AsFlexItem(
                size: new() { x = 70, y = 30 }
            ).Use();
        }
    }
} 
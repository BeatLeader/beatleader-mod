using System.Collections.Generic;
using BeatLeader.API;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Reactive.Yoga;
using ListView = Reactive.Components.Basic.ListView<BeatLeader.Models.NewsPost, BeatLeader.UI.MainMenu.TextNewsPostPanel>;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPanel : ReactiveComponent {

        private ObservableValue<WebRequests.RequestState> _requestState = null!;
        private ObservableValue<List<NewsPost>> _list = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            NewsRequest.SendRequest();
            NewsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDestroy() {
            NewsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<NewsPost>> instance, WebRequests.RequestState state, string? failReason) {
            _requestState.Value = state;
            if (state == WebRequests.RequestState.Finished) {
                _list.Value = instance.Result.data;
            }
        }

        protected override GameObject Construct() {
            _requestState = Remember(WebRequests.RequestState.Uninitialized);
            _list = Remember(new List<NewsPost>());

            return new Background {
                Children = {
                    new ScrollArea {
                        ScrollContent = new Layout {
                            Children = {
                                new Spinner()
                                    .AsFlexItem(size: new() { x = 8, y = 8 })
                                    .Animate(_requestState, (spinner, state) => spinner.Enabled = state == WebRequests.RequestState.Uninitialized || state == WebRequests.RequestState.Started),
                                new Label { RichText = true }
                                    .Animate(_list, (text, list) => {
                                        if (_requestState == WebRequests.RequestState.Finished && list.Count == 0) {
                                            text.Text = "There is no news";
                                            text.Enabled = true;
                                        } else if (_requestState == WebRequests.RequestState.Failed) {
                                            text.Text = "<color=#ff8888>Failed to load";
                                            text.Enabled = true;
                                        } else {
                                            text.Enabled = false;
                                        }
                                    }),
                                
                                new ListView()
                                    .Animate(_list, (table, list) => {
                                        if (list.Count == 0) {
                                            table.Enabled = false;
                                        } else {
                                            table.Items = list;
                                        }
                                    })
                                    .AsFlexItem(),
                            }
                        }
                        .AsFlexItem()
                        .AsFlexGroup(direction: FlexDirection.Column, constrainVertical: false),
                    }
                    .AsFlexItem(size: new() { x = 60, y = 70 })
                    .Export(out var scrollArea),
                    
                    new Scrollbar()
                        .AsFlexItem()
                        .With(x => scrollArea.Scrollbar = x)
                }
            }
            .AsFlexGroup(
                gap: 1f,
                padding: 1f
            ).AsBackground(
                color: Color.white.ColorWithAlpha(0.33f),
                pixelsPerUnit: 7f
            ).AsFlexItem(size: new() { x = 60, y = 70 })
            .Use();
        }
    }
}
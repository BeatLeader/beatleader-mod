using BeatLeader.API;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;
using Reactive.Yoga;

#pragma warning disable CS0618

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPanel : ReactiveComponent {
        #region Request

        protected override void OnInitialize() {
            NewsRequest.SendRequest();
            NewsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDestroy() {
            NewsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<NewsPost>> instance, WebRequests.RequestState state, string? failReason) {
            _newsPanel.UpdateFromRequest(
                state,
                state is WebRequests.RequestState.Finished ? instance.Result!.data : new()
            );
        }

        #endregion

        #region Construct

        private NewsPanel<NewsPost> _newsPanel = null!;

        protected override GameObject Construct() {
            return new Background {
                    FlexController = {
                        Padding = 1f,
                        JustifyContent = Justify.Center
                    },

                    FlexItem = {
                        Size = new() { x = 60, y = 70 }
                    },

                    Children = {
                        new NewsPanel<NewsPost> {
                            FlexController = {
                                Gap = 2f,
                                ConstrainVertical = false
                            },

                            FlexItem = {
                                FlexGrow = 1f
                            },

                            EmptyMessage = "There is no news",
                            ConstructCell = ctx => new TextNewsPostPanel(ctx)
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
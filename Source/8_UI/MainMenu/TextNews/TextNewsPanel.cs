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

        private NewsPanel<NewsPost, TextNewsPostPanel> _newsPanel = null!;

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new NewsPanel<NewsPost, TextNewsPostPanel> {
                                EmptyMessage = "There is no news"
                            }
                            .AsFlexGroup(gap: 2f, constrainVertical: false)
                            .AsFlexItem(flexGrow: 1f)
                            .Bind(ref _newsPanel)
                    }
                }
                .AsFlexGroup(
                    padding: 1f,
                    justifyContent: Justify.Center
                )
                .AsBackground(
                    color: Color.black.ColorWithAlpha(0.33f),
                    pixelsPerUnit: 7f
                )
                .AsFlexItem(size: new() { x = 60, y = 70 })
                .Use();
        }

        #endregion
    }
}
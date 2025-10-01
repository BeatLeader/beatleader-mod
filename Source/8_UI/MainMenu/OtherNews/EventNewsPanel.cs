using BeatLeader.API;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

#pragma warning disable CS0618

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : ReactiveComponent {
        #region Request

        protected override void OnInitialize() {
            PlatformEventsRequest.Send();
            PlatformEventsRequest.StateChangedEvent += OnRequestStateChanged;
        }

        protected override void OnDestroy() {
            PlatformEventsRequest.StateChangedEvent -= OnRequestStateChanged;
        }

        private void OnRequestStateChanged(WebRequests.IWebRequest<Paged<PlatformEvent>> instance, WebRequests.RequestState state, string? failReason) {
            _newsPanel.UpdateFromRequest(
                state,
                state is WebRequests.RequestState.Finished ? instance.Result!.data : new()
            );
        }

        #endregion

        #region Construct

        private NewsPanel<PlatformEvent, EventPreviewPanel> _newsPanel = null!;

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new NewsHeader {
                            Text = "BeatLeader Events"
                        }.AsFlexItem(),

                        new NewsPanel<PlatformEvent, EventPreviewPanel> {
                            EmptyMessage = "No events",
                            OnCellConstructed = cell => {
                                cell.ButtonAction = item => ReeModalSystem.OpenModal<EventDetailsDialog>(ContentTransform, item);
                                cell.BackgroundAction = item => ReeModalSystem.OpenModal<EventDetailsDialog>(ContentTransform, item);
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
                .AsFlexItem(size: new() { x = 70, y = 30 })
                .Use();
        }

        #endregion
    }
}
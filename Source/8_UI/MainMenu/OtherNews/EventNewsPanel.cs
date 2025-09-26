using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

#pragma warning disable CS0618

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : ReactiveComponent {
        #region Public API

        public void SetData(RequestState state, IReadOnlyList<PlatformEvent>? events) {
            _newsPanel.UpdateFromRequest(state, events);
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
                .AsFlexItem(size: new() { x = 70 }, flexGrow: 1f)
                .Use();
        }

        #endregion
    }
}
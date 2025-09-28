using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventNewsPanel : ReactiveComponent {
        #region Public API

        public Action<PlatformEvent>? OnEventSelected;

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
                                cell.OnClick = x => OnEventSelected?.Invoke(x);
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
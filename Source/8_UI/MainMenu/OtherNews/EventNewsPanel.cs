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

        private NewsPanel<PlatformEvent> _newsPanel = null!;

        protected override GameObject Construct() {
            return new Background {
                    FlexController = {
                        Padding = 1f,
                        FlexDirection = FlexDirection.Column
                    },

                    FlexItem = {
                        Size = new() { x = 70 },
                        FlexGrow = 1f
                    },

                    Children = {
                        new NewsHeader {
                            Text = "BeatLeader Events"
                        },

                        new NewsPanel<PlatformEvent> {
                            FlexItem = {
                                FlexGrow = 1f
                            },

                            EmptyMessage = "No events",
                            ConstructCell = ctx => new EventPreviewPanel(ctx) {
                                OnClick = x => OnEventSelected?.Invoke(x)
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
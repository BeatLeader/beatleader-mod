using System;
using System.Collections.Generic;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using static BeatLeader.WebRequests.RequestState;

namespace BeatLeader.UI.MainMenu {
    internal class NewsPanel<TItem, TCell> : ReactiveComponent where TCell : IListCell<TItem>, IReactiveComponent, new() {
        #region Props

        public string EmptyMessage { get; set; } = "Nothing to show";
        public Action<TCell>? OnCellConstructed { get; set; }

        public void UpdateFromRequest(RequestState state, List<TItem> items) {
            _requestState.Value = state;
            _list.Value = items;
        }

        #endregion

        #region Construct

        private ObservableValue<RequestState> _requestState = null!;
        private ObservableValue<List<TItem>> _list = null!;

        protected override GameObject Construct() {
            _requestState = Remember(Uninitialized);
            _list = Remember(new List<TItem>());

            var transitionAlpha = RememberAnimated(0f, 200.ms());

            _requestState.ValueChangedEvent += x => {
                transitionAlpha.Value = x is Finished or Failed ? 1f : 0f;
            };

            return new Layout {
                Children = {
                    new Spinner {
                            Image = {
                                RaycastTarget = false
                            }
                        }
                        .Animate(transitionAlpha, (spinner, val) => spinner.Image.Color = Color.white.ColorWithAlpha(1f - val))
                        .AsFlexItem(
                            size: 8f,
                            alignSelf: Align.Center,
                            positionType: PositionType.Absolute
                        ),

                    new Layout {
                            Children = {
                                new Label { RichText = true }
                                    .Animate(
                                        _list,
                                        (text, list) => {
                                            switch (_requestState.Value) {
                                                case Finished when list.Count == 0:
                                                    text.Text = EmptyMessage;
                                                    text.Enabled = true;
                                                    break;

                                                case Failed:
                                                    text.Text = "<color=#ff8888>Failed to load";
                                                    text.Enabled = true;
                                                    break;

                                                default:
                                                    text.Enabled = false;
                                                    break;
                                            }
                                        },
                                        applyImmediately: true
                                    )
                                    .AsFlexItem(
                                        alignSelf: Align.Center,
                                        positionType: PositionType.Absolute
                                    ),

                                new ScrollArea {
                                        ScrollContent =
                                            new ListView<TItem, TCell> {
                                                    WhenCellConstructed = x => OnCellConstructed?.Invoke(x)
                                                }
                                                .Animate(_list, (table, items) => table.Items = items)
                                                .AsFlexGroup(
                                                    gap: 1,
                                                    direction: FlexDirection.Column,
                                                    constrainVertical: false
                                                ),
                                    }
                                    .AsFlexItem(flexGrow: 1)
                                    .Export(out var scrollArea),

                                new Scrollbar()
                                    .AsFlexItem()
                                    .With(x => scrollArea.Scrollbar = x)
                            }
                        }
                        .WithNativeComponent(out CanvasGroup group)
                        .Animate(transitionAlpha, (_, y) => group.alpha = y, applyImmediately: true)
                        .AsFlexItem(flexGrow: 1f)
                        .AsFlexGroup(gap: 1f, justifyContent: Justify.Center)
                }
            }.AsFlexGroup(justifyContent: Justify.Center).Use();
        }

        #endregion
    }
}
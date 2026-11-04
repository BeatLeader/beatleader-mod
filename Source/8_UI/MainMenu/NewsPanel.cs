using System;
using System.Collections.Generic;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using static BeatLeader.WebRequests.RequestState;

namespace BeatLeader.UI.MainMenu {
    internal class NewsPanel<TItem> : ReactiveComponent, ILayoutControllerHolder {
        #region Props

        public string EmptyMessage { get; set; } = "Nothing to show";

        public Func<CellContext<TItem>, IReactiveComponent> ConstructCell { get; set; } = null!;

        public void UpdateFromRequest(RequestState state, IReadOnlyList<TItem>? items) {
            _requestState.Value = state;
            _list.Value = items ?? Array.Empty<TItem>();
        }

        #endregion

        #region Layout Controller

        public ILayoutController? LayoutController {
            get => _repeater.LayoutController;
            set => _repeater.LayoutController = value;
        }

        #endregion

        #region Construct

        private State<RequestState> _requestState = null!;
        private State<IReadOnlyList<TItem>> _list = null!;
        private Repeater<TItem, IReactiveComponent> _repeater = null!;

        protected override GameObject Construct() {
            _requestState = Remember(Uninitialized);
            _list = Remember<IReadOnlyList<TItem>>(Array.Empty<TItem>());

            var transitionAlpha = RememberAnimated(0f, 200.ms);
            var showMessage = RememberDerived(
                _requestState, _list,
                static (state, list) => state is Failed || (state is Finished && list.Count == 0)
            );
            var scrollContext = new ScrollContext();

            _requestState.AddCallback(x => transitionAlpha.TargetValue = x is Finished or Failed ? 1f : 0f, lazy: true);

            return new Layout {
                FlexController = {
                    JustifyContent = Justify.Center
                },

                Children = {
                    new Spinner {
                        FlexItem = {
                            Size = 8f,
                            AlignSelf = Align.Center,
                            PositionType = PositionType.Absolute
                        },

                        Image = {
                            RaycastTarget = false,
                            sColor = transitionAlpha.Map(x => Color.white.ColorWithAlpha(1f - x))
                        }
                    },

                    new Layout {
                            FlexController = {
                                Gap = 1f,
                                JustifyContent = Justify.Center
                            },

                            FlexItem = {
                                FlexGrow = 1f
                            },

                            Children = {
                                new Label {
                                    FlexItem = {
                                        AlignSelf = Align.Center,
                                        PositionType = PositionType.Absolute
                                    },

                                    RichText = true,
                                    sEnabled = showMessage,
                                    sText = _requestState.Map(x => x is Failed ? "<color=#ff8888>Failed to load" : EmptyMessage)
                                },

                                new ScrollArea {
                                    FlexItem = {
                                        FlexGrow = 1
                                    },

                                    ScrollContext = scrollContext,
                                    LineSize = 1f,
                                    ScrollContent =
                                        new Repeater<TItem, IReactiveComponent> {
                                            FlexController = {
                                                FlexDirection = FlexDirection.Column,
                                                ConstrainVertical = false
                                            },

                                            sItems = _list,
                                            ConstructCell = x => ConstructCell(x)
                                        }.Bind(ref _repeater),
                                },

                                new Scrollbar {
                                    ScrollContext = scrollContext,
                                    HideIfNothingToScroll = true
                                }
                            }
                        }
                        .WithAlpha(transitionAlpha)
                }
            }.Use();
        }

        #endregion
    }
}
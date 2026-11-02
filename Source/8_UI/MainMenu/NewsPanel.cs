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
                Children = {
                    new Spinner {
                            Image = {
                                RaycastTarget = false,
                                sColor = transitionAlpha.Map(x => Color.white.ColorWithAlpha(1f - x))
                            }
                        }
                        .AsFlexItem(
                            size: 8f,
                            alignSelf: Align.Center,
                            positionType: PositionType.Absolute
                        ),

                    new Layout {
                            Children = {
                                new Label {
                                        RichText = true,
                                        sEnabled = showMessage,
                                        sText = _requestState.Map(x => x is Failed ? "<color=#ff8888>Failed to load" : EmptyMessage)
                                    }
                                    .AsFlexItem(
                                        alignSelf: Align.Center,
                                        positionType: PositionType.Absolute
                                    ),

                                new ScrollArea {
                                        ScrollContext = scrollContext,
                                        LineSize = 1f,
                                        ScrollContent =
                                            new Repeater<TItem, IReactiveComponent> {
                                                    sItems = _list,
                                                    ConstructCell = x => ConstructCell(x)
                                                }
                                                .AsFlexGroup(
                                                    direction: FlexDirection.Column,
                                                    constrainVertical: false
                                                )
                                                .Bind(ref _repeater),
                                    }
                                    .AsFlexItem(flexGrow: 1),

                                new Scrollbar {
                                        ScrollContext = scrollContext,
                                        HideIfNothingToScroll = true
                                    }
                                    .AsFlexItem()
                            }
                        }
                        .WithNativeComponent(out CanvasGroup group)
                        .On(transitionAlpha, (_, y) => group.alpha = y)
                        .AsFlexItem(flexGrow: 1f)
                        .AsFlexGroup(gap: 1f, justifyContent: Justify.Center)
                }
            }.AsFlexGroup(justifyContent: Justify.Center).Use();
        }

        #endregion
    }
}
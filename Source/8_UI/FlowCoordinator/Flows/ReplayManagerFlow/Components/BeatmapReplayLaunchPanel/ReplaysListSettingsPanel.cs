using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListSettingsPanel : ReactiveComponent {
        internal class SortOptionsModal : ModalBase {
            #region Setup

            public IReplaysList? ReplaysList {
                get => _replaysList.Value;
                set => _replaysList.Value = value;
            }

            #endregion

            #region Construct

            private State<IReplaysList?> _replaysList = null!;

            protected override IReactiveComponent ConstructContent() {
                _replaysList = Remember<IReplaysList?>(null);

                var sorter = Remember(ReplaysListSorter.Date);
                var sortOrder = Remember(SortOrder.Ascending);

                // A list set later gets the options chosen before it
                _replaysList.AddCallback(
                    x => {
                        x?.Sorter = sorter.Value;
                        x?.SortOrder = sortOrder.Value;
                    },
                    lazy: true
                );

                sorter.AddCallback(x => _replaysList.Value?.Sorter = x, lazy: true);
                sortOrder.AddCallback(x => _replaysList.Value?.SortOrder = x, lazy: true);

                return new Background {
                    FlexItem = {
                        Size = new() { x = 60.pt, y = 20.pt }
                    },

                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        JustifyContent = Justify.SpaceEvenly,
                        Padding = 2.pt
                    },

                    Children = {
                        // Sorter selector
                        new NamedRail {
                            FlexItem = {
                                FlexBasis = 6f,
                            },

                            Label = {
                                Text = "Sort By",
                            },

                            Component = new BsDropdown<ReplaysListSorter> {
                                Skew = 0f,
                                
                                Items = new Dictionary<ReplaysListSorter, BsDropdownItem> {
                                    [ReplaysListSorter.Date] = new("Date", null),
                                    [ReplaysListSorter.Player] = new("Player", null),
                                    [ReplaysListSorter.Completion] = new("Completion", null),
                                    [ReplaysListSorter.Difficulty] = new("Difficulty", null)
                                },
                                
                                Key = sorter.Value,
                                OnKeyChanged = x => sorter.Value = x,
                            },
                        },

                        // Sort order selector
                        new NamedRail {
                            FlexItem = {
                                FlexBasis = 6f,
                            },

                            Label = {
                                Text = "Order",
                            },

                            Component = new BsSegmentedControl<SortOrder> {
                                FlexItem = {
                                    FlexGrow = 1f,
                                    Size = new() { y = 6.pt },
                                    Margin = new() { left = 2.pt }
                                },

                                Skew = 0f,
                                
                                Items = new Dictionary<SortOrder, BsSegmentedControlItem> {
                                    [SortOrder.Ascending] = new(null, BundleLoader.Sprites.ascendingIcon),
                                    [SortOrder.Descending] = new(null, BundleLoader.Sprites.descendingIcon),
                                },
                                
                                Key = sortOrder.Value,
                                OnKeyChanged = x => sortOrder.Value = x,
                            },
                        }
                    }
                }.AsBlurBackground();
            }

            #endregion
        }

        #region Setup

        public IReplaysList? ReplaysList {
            get => _replaysList.Value;
            set => _replaysList.Value = value;
        }

        #endregion

        #region Construct

        private State<IReplaysList?> _replaysList = null!;

        protected override GameObject Construct() {
            var anchor = Ref<RectTransform>();
            var sortOptionsOpened = Remember(false);

            _replaysList = Remember<IReplaysList?>(null);

            return new Background {
                FlexController = {
                    Padding = 0.6f,
                    Gap = 0.5f
                },

                Children = {
                    new SortOptionsModal {
                        PlacementData = new() { Placement = RelativePlacement.TopCenter, Offset = new(0f, 0.5f) },
                        PlacementAnchor = anchor,
                        CompositionAnimation = new() { DimCanvas = false },

                        sReplaysList = _replaysList,

                        sIsPushed = sortOptionsOpened,
                        OnPopped = () => sortOptionsOpened.Value = false,
                        OnClickOutside = () => sortOptionsOpened.Value = false
                    },

                    new Label {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Text = "List settings",
                        Alignment = TextAlignmentOptions.MidlineLeft,
                        FontSize = 3f
                    },

                    // Refresh button
                    CreateGlowingButton(
                        BundleLoader.RotateRightIcon,
                        () => {
                            ReplayManager.ClearHeadersCache();
                            ReplayManager.StartLoading();
                        }
                    ),

                    // Settings button
                    CreateGlowingButton(
                        BundleLoader.SettingsIcon,
                        () => sortOptionsOpened.Value = true
                    ),
                }
            }.AsBlurBackground().Bind(anchor).Use();
        }

        [StateGen]
        private static Image CreateGlowingButton(Sprite sprite, Action onClick) {
            var graphicState = Remember(GraphicState.None);

            return new Image {
                FlexItem = {
                    AspectRatio = 1,
                },

                Sprite = sprite,
                Material = BundleLoader.UIAdditiveGlowMaterial,
                sColor = graphicState.MapColorSet(UIStyle.GlowingButtonColorSet)
            }.WithPointerEvents(
                onEnter: _ => graphicState.IsHovered = true,
                onLeave: _ => graphicState.IsHovered = false,
                onDown: _ => {
                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                    onClick();
                }
            );
        }

        #endregion
    }
}
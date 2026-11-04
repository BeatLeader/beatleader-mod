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
        private class SortOptionsModal : ModalBase {
            #region Setup

            private IReplaysList? _replaysList;

            public void Setup(IReplaysList replaysList) {
                _replaysList = replaysList;
                _replaysList.Sorter = _sortersDropdown.Key;
                _replaysList.SortOrder = _sortOrderSelector.Key;
            }

            #endregion

            #region Construct

            private BsDropdown<ReplaysListSorter> _sortersDropdown = null!;
            private BsSegmentedControl<SortOrder> _sortOrderSelector = null!;

            protected override IReactiveComponent ConstructContent() {
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
                        //sorter selector
                        new NamedRail {
                            FlexItem = {
                                FlexBasis = 6f
                            },

                            Label = {
                                Text = "Sort By"
                            },
                            Component = new BsDropdown<ReplaysListSorter> {
                                Skew = 0f,
                                Items = new Dictionary<ReplaysListSorter, BsDropdownItem> {
                                    [ReplaysListSorter.Date] = new("Date", null),
                                    [ReplaysListSorter.Player] = new("Player", null),
                                    [ReplaysListSorter.Completion] = new("Completion", null),
                                    [ReplaysListSorter.Difficulty] = new("Difficulty", null)
                                },
                                Key = ReplaysListSorter.Date,
                                OnKeyChanged = x => _replaysList?.Sorter = x
                            }.Bind(ref _sortersDropdown)
                        },
                        //sort order selector
                        new NamedRail {
                            FlexItem = {
                                FlexBasis = 6f
                            },

                            Label = {
                                Text = "Order"
                            },
                            Component = new Background {
                                FlexItem = {
                                    Size = new() { x = 36f }
                                },

                                Children = {
                                    new BsSegmentedControl<SortOrder> {
                                        FlexController = {
                                            Padding = 1f
                                        },

                                        FlexItem = {
                                            FlexGrow = 1f,
                                            Size = new() { y = 100.pct }
                                        },

                                        Items = new Dictionary<SortOrder, BsSegmentedControlItem> {
                                            [SortOrder.Ascending] = new(null, BundleLoader.Sprites.ascendingIcon),
                                            [SortOrder.Descending] = new(null, BundleLoader.Sprites.descendingIcon)
                                        },
                                        Key = SortOrder.Ascending,
                                        OnKeyChanged = x => _replaysList?.SortOrder = x
                                    }.Bind(ref _sortOrderSelector)
                                }
                            }.AsFlexGroup().AsBlurBackground(
                                color: Color.white.ColorWithAlpha(0.8f)
                            )
                        }
                    }
                }.AsBlurBackground();
            }

            protected override void OnInitialize() {
                base.OnInitialize();
                OnClickOutside = () => IsPushed = false;
            }

            #endregion
        }

        #region Construct

        private SortOptionsModal _sortOptionsModal = null!;

        protected override GameObject Construct() {
            return new Background {
                FlexController = {
                    Padding = 0.6f,
                    Gap = 0.5f
                },

                Children = {
                    new SortOptionsModal {
                        PlacementData = new() { Placement = RelativePlacement.TopRight, Offset = new(0f, 0.5f) }
                    }.Bind(ref _sortOptionsModal),
                    //label
                    new Label {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Text = "List settings",
                        Alignment = TextAlignmentOptions.MidlineLeft,
                        FontSize = 3f
                    },
                    //refresh button
                    CreateGlowingButton(
                        BundleLoader.RotateRightIcon,
                        () => {
                            ReplayManager.ClearHeadersCache();
                            ReplayManager.StartLoading();
                        }
                    ).AsFlexItem(aspectRatio: 1f),
                    //settings button
                    CreateGlowingButton(
                        BundleLoader.SettingsIcon,
                        () => _sortOptionsModal.IsPushed = true
                    ).AsFlexItem(aspectRatio: 1f)
                }
            }.AsBlurBackground().Use();
        }

        [StateGen]
        private static Image CreateGlowingButton(Sprite sprite, Action onClick) {
            var graphicState = Remember(GraphicState.None);

            return new Image {
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

        protected override void OnInitialize() {
            _sortOptionsModal.PlacementAnchor = ContentTransform;
        }

        #endregion

        #region Setup

        public void Setup(IReplaysList replaysList) {
            _sortOptionsModal.Setup(replaysList);
        }

        #endregion
    }
}
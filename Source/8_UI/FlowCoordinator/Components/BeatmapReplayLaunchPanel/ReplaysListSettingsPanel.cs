﻿using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysListSettingsPanel : ReactiveComponent {
        private class SortOptionsModal : AnimatedModalComponentBase {
            #region Setup

            private IReplaysList? _replaysList;

            public void Setup(IReplaysList replaysList) {
                _replaysList = replaysList;
                _replaysList.Sorter = _sortersDropdown.SelectedKey;
                _replaysList.SortOrder = _sortOrderSelector.SelectedKey;
            }

            #endregion

            #region Construct

            private TextDropdown<ReplaysListSorter> _sortersDropdown = null!;
            private IconSegmentedControl<SortOrder> _sortOrderSelector = null!;
            
            protected override GameObject Construct() {
                return new Image {
                    Children = {
                        //sorter selector
                        new NamedRail {
                            Label = {
                                Text = "Sort By"
                            },
                            Component = new TextDropdown<ReplaysListSorter> {
                                Skew = 0f,
                                Items = {
                                    { ReplaysListSorter.Date, "Date" },
                                    { ReplaysListSorter.Player, "Player" },
                                    { ReplaysListSorter.Completion, "Completion" },
                                    { ReplaysListSorter.Difficulty, "Difficulty" },
                                }
                            }.WithListener(
                                x => x.SelectedKey,
                                x => {
                                    if (_replaysList == null) return;
                                    _replaysList.Sorter = x;
                                }
                            ).Bind(ref _sortersDropdown)
                        }.AsFlexItem(basis: 6f),
                        //sort order selector
                        new NamedRail {
                            Label = {
                                Text = "Order"
                            },
                            Component = new Image {
                                Children = {
                                    new IconSegmentedControl<SortOrder> {
                                        Items = {
                                            { SortOrder.Ascending, BundleLoader.Sprites.ascendingIcon },
                                            { SortOrder.Descending, BundleLoader.Sprites.descendingIcon }
                                        }
                                    }.WithListener(
                                        x => x.SelectedKey,
                                        x => {
                                            if (_replaysList == null) return;
                                            _replaysList.SortOrder = x;
                                        }
                                    ).AsFlexGroup(padding: 1f).WithRectExpand().Bind(ref _sortOrderSelector)
                                }
                            }.AsFlexItem(
                                size: new() { x = 36f }
                            ).AsBlurBackground(
                                color: Color.white.ColorWithAlpha(0.8f)
                            )
                        }.AsFlexItem(basis: 6f)
                        //named rail
                    }
                }.AsFlexGroup(
                    direction: FlexDirection.Column,
                    padding: 2f
                ).AsBlurBackground().WithSizeDelta(60f, 20f).Use();
            }

            #endregion
        }

        #region Construct

        private SortOptionsModal _sortOptionsModal = null!;
        private ImageButton _settingsButton = null!;

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    new SortOptionsModal()
                        .WithAnchor(() => ContentTransform, RelativePlacement.TopRight)
                        .Bind(ref _sortOptionsModal),
                    //label
                    new Label {
                        Text = "List settings",
                        Alignment = TextAlignmentOptions.MidlineLeft,
                        FontSize = 3f
                    }.AsFlexItem(grow: 1f),
                    //refresh button
                    new ImageButton {
                        Image = {
                            Sprite = BundleLoader.RotateRightIcon
                        },
                        OnClick = () => _replaysLoader!.StartReplaysLoad()
                    }.AsFlexItem(aspectRatio: 1f),
                    //settings button
                    new ImageButton {
                        Image = {
                            Sprite = BundleLoader.SettingsIcon
                        }
                    }.WithModal(_sortOptionsModal).AsFlexItem(
                        aspectRatio: 1f
                    ).Bind(ref _settingsButton)
                }
            }.AsFlexGroup(padding: 0.6f, gap: 0.5f).AsBlurBackground().Use();
        }

        #endregion

        #region Setup

        private IReplaysLoader? _replaysLoader;

        public void Setup(IReplaysList replaysList, IReplaysLoader replaysLoader) {
            _replaysLoader = replaysLoader;
            _sortOptionsModal.Setup(replaysList);
        }

        #endregion
    }
}
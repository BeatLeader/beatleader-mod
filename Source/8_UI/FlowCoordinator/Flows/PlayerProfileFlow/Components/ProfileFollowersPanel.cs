using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;
using SelectionMode = Reactive.Components.Basic.SelectionMode;

namespace BeatLeader.UI.Hub {
    internal class ProfileFollowersPanel : ReactiveComponent {
        #region Props

        public string? PlayerId {
            get => _request.Value.PlayerId;
            set => _request.Value = _request.Value with { PlayerId = value, Page = 1 };
        }

        public bool Following {
            get => _request.Value.Following;
            set => _request.Value = _request.Value with { Following = value, Page = 1 };
        }

        public int ExpectedCount {
            get => _expectedCount;
            set {
                _expectedCount = value;
                if (_players.Value.Count == 0) _listHeight.SetValueImmediate(GetExpectedHeight());
            }
        }

        public Action<PlayerFollower>? OnPlayerSelected { get; set; }

        #endregion

        #region Loading

        private const int PlayersPerPage = 6;
        private const float RowHeight = 5.5f;
        private const float RowGap = 0.5f;

        private readonly record struct FollowersRequest(string? PlayerId, bool Following, int Page);

        private (string? PlayerId, bool Following) _loadedList;
        private int _expectedCount;

        private static float GetListHeight(int count) {
            return Mathf.Max(count, 1) * (RowHeight + RowGap) - RowGap;
        }

        private float GetExpectedHeight() {
            return GetListHeight(Mathf.Min(_expectedCount, PlayersPerPage));
        }

        private async Task LoadAsync(FollowersRequest request) {
            if ((request.PlayerId, request.Following) != _loadedList) {
                _loadedList = (request.PlayerId, request.Following);
                _players.Value = [];
                _listHeight.SetValueImmediate(GetExpectedHeight());
            }

            if (request.PlayerId == null) {
                return;
            }

            _loading.Value = true;

            var result = await PlayerFollowersRequest.Send(request.PlayerId, request.Following, request.Page, PlayersPerPage).Join();

            if (request != _request.Value) {
                return;
            }

            _loading.Value = false;

            var players = (IReadOnlyList<PlayerFollower>?)result.Result ?? [];

            _listHeight.TargetValue = GetListHeight(players.Count);

            if (players.Select(static x => x.id).SequenceEqual(_players.Value.Select(static x => x.id))) {
                return;
            }

            _players.Value = players;
            ProfileReveal.Restart(_time);
        }

        #endregion

        #region Construct

        private State<FollowersRequest> _request = null!;
        private State<IReadOnlyList<PlayerFollower>> _players = null!;
        private State<bool> _loading = null!;
        private AnimatedState<float> _time = null!;
        private AnimatedState<float> _listHeight = null!;

        protected override GameObject Construct() {
            _request = Remember(new FollowersRequest(null, false, 1));
            _players = Remember<IReadOnlyList<PlayerFollower>>([]);
            _loading = Remember(false);
            _time = RememberAnimated(0f, ProfileReveal.TotalTime.sec);

            _listHeight = RememberAnimated(GetListHeight(0), 250.ms, AnimationCurve.EaseOutQuint);

            _request.AddCallback(this, static (x, y) => _ = x.LoadAsync(y).RunCatching());

            var listAlpha = RememberAnimated(_loading.Map(static x => x ? 0.3f : 1f), 150.ms);
            var emptyShown = RememberDerived(_players, _loading, static (x, y) => x.Count == 0 && !y);

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Padding = 1f,
                    Gap = 1f
                },

                FlexItem = {
                    Size = new() { x = 46f }
                },

                Children = {
                    new Layout {
                        FlexController = {
                            AlignItems = Align.Center,
                            Padding = new() { left = 0.5f },
                            Gap = 1f
                        },

                        FlexItem = {
                            Size = new() { y = 4f }
                        },

                        Children = {
                            new Label {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                FontSize = 3.2f,
                                FontStyle = FontStyles.Bold,
                                Alignment = TextAlignmentOptions.CaplineLeft,
                                sText = _request.Map(static x => x.Following ? "Following" : "Followers")
                            },

                            new BsTextButton {
                                FlexItem = {
                                    Size = new() { x = 4f, y = 4f }
                                },

                                Text = "<",
                                sInteractable = _request.Map(static x => x.Page > 1),
                                OnClick = () => _request.Value = _request.Value with { Page = _request.Value.Page - 1 }
                            },

                            new Label {
                                FlexItem = {
                                    Size = new() { x = 4f }
                                },

                                FontSize = 2.8f,
                                Alignment = TextAlignmentOptions.Capline,
                                sText = _request.Map(static x => x.Page.ToString())
                            },

                            new BsTextButton {
                                FlexItem = {
                                    Size = new() { x = 4f, y = 4f }
                                },

                                Text = ">",
                                sInteractable = _players.Map(static x => x.Count == PlayersPerPage),
                                OnClick = () => _request.Value = _request.Value with { Page = _request.Value.Page + 1 }
                            }
                        }
                    },

                    new Repeater<PlayerFollower, Background> {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            Gap = RowGap
                        },

                        FlexItem = {
                            sSize = _listHeight.Map(static x => new YogaVector { y = x })
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = _players,

                        ConstructCell = ctx => {
                            var stage = RememberDerived(ctx, _time, static (cell, x) => ProfileReveal.Progress(x, cell.Index * ProfileReveal.ItemDelay * 1.5f));
                            var graphicState = Remember(GraphicState.None);

                            return new Background {
                                FlexController = {
                                    AlignItems = Align.Center,
                                    Padding = 0.5f,
                                    Gap = 2f
                                },

                                FlexItem = {
                                    Size = new() { y = RowHeight },
                                    sPosition = stage.Map(static x => new YogaFrame { left = (1f - x) * 6f })
                                },

                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 10f,
                                sColor = graphicState.MapColorSet(BeatSaberStyle.BsSegmentedControlColors.CellBackgroundColors),

                                Children = {
                                    new SmallAvatar {
                                        FlexItem = {
                                            Size = 4.5f
                                        },

                                        sUrl = ctx.Map(static x => x.Item.avatar)
                                    },

                                    new Label {
                                        FlexItem = {
                                            FlexGrow = 1f,
                                            FlexShrink = 1f
                                        },

                                        FontSize = 2.6f,
                                        Alignment = TextAlignmentOptions.CaplineLeft,
                                        EnableWrapping = false,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        RichText = false,
                                        sText = ctx.Map(static x => x.Item.name)
                                    },

                                    new ProfileTag {
                                        FontSize = 2f,
                                        Text = "Mutual",
                                        Color = new(0.42f, 0.8f, 1f),
                                        sEnabled = ctx.Map(static x => x.Item.mutual)
                                    }
                                }
                            }.WithAlpha(stage).WithPointerEvents(
                                onEnter: _ => graphicState.IsHovered = true,
                                onLeave: _ => graphicState.IsHovered = false,
                                onDown: _ => {
                                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                    OnPlayerSelected?.Invoke(ctx.Item);
                                }
                            );
                        }
                    }.WithAlpha(listAlpha).AsRectMask(),

                    new Label {
                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = new() { left = 0f, right = 0f, bottom = 1.5f },
                            Size = new() { y = RowHeight }
                        },

                        FontSize = 3f,
                        Color = BeatSaberStyle.SecondaryTextColor,
                        Text = "Nobody here yet",
                        sEnabled = emptyShown
                    }
                }
            }.Use();
        }

        #endregion
    }
}

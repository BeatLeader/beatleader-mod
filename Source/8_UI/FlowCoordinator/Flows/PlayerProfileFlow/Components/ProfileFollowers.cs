using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using SelectionMode = Reactive.Components.Basic.SelectionMode;

namespace BeatLeader.UI.Hub {
    internal class ProfileFollowers : ReactiveComponent {
        #region Props

        public string? PlayerId {
            get => _playerId.Value;
            set => _playerId.Value = value;
        }

        public PlayerFollowersInfo? Info {
            get => _info.Value;
            set => _info.Value = value;
        }

        #endregion

        #region Construct

        private State<string?> _playerId = null!;
        private State<PlayerFollowersInfo?> _info = null!;

        protected override GameObject Construct() {
            _playerId = Remember<string?>(null);
            _info = Remember<PlayerFollowersInfo?>(null);

            var shown = Remember(false);
            var following = Remember(false);
            var expectedCount = RememberDerived(_info, following, static (x, y) => (y ? x?.followingCount : x?.followersCount) ?? 0);
            var anchor = Ref<RectTransform>();

            void Open(bool value) {
                following.Value = value;
                shown.Value = true;
            }

            return new Layout {
                FlexController = {
                    AlignItems = Align.Center,
                    Gap = 4f
                },

                Children = {
                    new Modal {
                        PlacementAnchor = anchor,
                        PlacementData = new() { Placement = RelativePlacement.Center },

                        sIsPushed = shown,
                        OnPopped = () => shown.Value = false,
                        OnClickOutside = () => shown.Value = false,

                        Children = {
                            new Background {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column
                                },

                                Children = {
                                    new ProfileFollowersPanel {
                                        sPlayerId = _playerId,
                                        sFollowing = following,
                                        sExpectedCount = expectedCount,

                                        OnPlayerSelected = x => {
                                            shown.Value = false;
                                            PlayerProfileFlowCoordinator.Open(x.id);
                                        }
                                    }
                                }
                            }.AsBlurBackground().AsRectMask()
                        }
                    },

                    CreateButton(
                        _info.Map(static x => $"<b>{x?.followingCount ?? 0}</b> Following"),
                        _info.Map(static x => (IReadOnlyList<string>)(x?.following ?? [])),
                        () => Open(true)
                    ),

                    CreateButton(
                        _info.Map(static x => $"<b>{x?.followersCount ?? 0}</b> Followers"),
                        _info.Map(static x => (IReadOnlyList<string>)(x?.followers ?? [])),
                        () => Open(false)
                    )
                }
            }.Bind(anchor).Use();
        }

        [StateGen]
        private static Background CreateButton(IState<string> text, IState<IReadOnlyList<string>> avatars, Action onClick) {
            return new Background {
                FlexController = {
                    AlignItems = Align.Center,
                    Gap = 1.5f
                },

                Color = Color.clear,

                Children = {
                    new Label {
                        FontSize = 3f,
                        sText = text.In()
                    },

                    new Repeater<string, SmallAvatar> {
                        FlexController = {
                            FlexDirection = FlexDirection.Row
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = avatars.In(),

                        ConstructCell = ctx => new SmallAvatar {
                            FlexItem = {
                                Size = 5f,
                                Margin = new() { right = -1.5f }
                            },

                            sUrl = ctx.Map(static x => (string?)x.Item)
                        }
                    }
                }
            }.WithHoverScale(1f, 1.08f).WithPointerEvents(
                onDown: _ => {
                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                    onClick();
                }
            );
        }

        #endregion
    }
}

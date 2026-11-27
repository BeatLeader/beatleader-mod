using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal class ProfileOverviewPanel(PlayerProfileContext context) : ReactiveComponent {
        #region Construct

        private const int MaxBadges = 8;

        private AnimatedState<float> _time = null!;

        protected override GameObject Construct() {
            _time = RememberAnimated(0f, ProfileReveal.TotalTime.sec);

            var player = context.Map(static x => x.Player);
            var status = context.Map(static x => (x.IsLoading, x.IsFailed));

            var badges = player.Map(static x => (IReadOnlyList<PlayerBadge>)(x?.badges ?? [])
                .Where(static y => !y.image.EndsWith(".webp"))
                .Take(MaxBadges)
                .ToArray()
            );

            var selectedBadge = Remember<ProfileBadgeOverview.Selection?>(null);
            var badgeProgress = RememberAnimated(0f, 450.ms, AnimationCurve.EaseOutQuint);
            var hiddenBadge = RememberDerived(selectedBadge, badgeProgress, static (x, y) => y > 0.001f ? x?.Badge : null);
            var contentAlpha = badgeProgress.Map(static x => 1f - x * 0.8f);

            player.AddCallback(this, static (x, y) => {
                if (y != null) {
                    ProfileReveal.Restart(x._time);
                }
            });

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart,
                    AlignItems = Align.Center
                },

                Children = {
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            Gap = 1.5f
                        },

                        FlexItem = {
                            Size = new() { x = 116f }
                        },

                        sEnabled = player.Map(static x => x != null),

                        Children = {
                            new ProfileHeader {
                                sPlayer = player,
                                sFollowers = context.Map(static x => x.Followers),
                                sPreviousPlayer = context.Map(static x => x.PreviousPlayer),
                                sTime = _time,
                                OnBack = context.Back
                            },

                            new ProfileBadges {
                                sBadges = badges,
                                sHiddenBadge = hiddenBadge,
                                sTime = _time,
                                sEnabled = badges.Map(static x => x.Count > 0),

                                OnBadgeSelected = (badge, source) => {
                                    selectedBadge.Value = new(badge, source);
                                    badgeProgress.TargetValue = 1f;
                                }
                            },

                            new ProfileStats {
                                sPlayer = player,
                                sTime = _time
                            }
                        }
                    }.WithAlpha(contentAlpha),

                    new ProfileBadgeOverview {
                        sSelectedBadge = selectedBadge,
                        sProgress = badgeProgress,
                        OnClose = () => badgeProgress.TargetValue = 0f
                    },

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Center,
                            Gap = 2f
                        },

                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = 0f
                        },

                        sEnabled = status.Map(static x => x.IsLoading || x.IsFailed),

                        Children = {
                            new Spinner {
                                FlexItem = {
                                    Size = 7f
                                },

                                sEnabled = status.Map(static x => x.IsLoading)
                            },

                            new Label {
                                FontSize = 3.5f,
                                Color = BeatSaberStyle.SecondaryTextColor,
                                Text = "Monke couldn't load the profile",
                                sEnabled = status.Map(static x => x.IsFailed)
                            },

                            new BsTextButton {
                                Text = "Retry",
                                sEnabled = status.Map(static x => x.IsFailed),
                                OnClick = () => {
                                    if (context.PlayerId != null) context.Load(context.PlayerId);
                                }
                            }
                        }
                    }
                }
            }.Use();
        }

        #endregion
    }
}

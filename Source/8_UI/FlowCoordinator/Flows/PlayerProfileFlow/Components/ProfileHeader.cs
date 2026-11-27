using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using SelectionMode = Reactive.Components.Basic.SelectionMode;

namespace BeatLeader.UI.Hub {
    internal class ProfileHeader : ReactiveComponent {
        #region Props

        public Player? Player {
            get => _player.Value;
            set => _player.Value = value;
        }

        public PlayerFollowersInfo? Followers {
            get => _followers.Value;
            set => _followers.Value = value;
        }

        public float Time {
            get => _time.Value;
            set => _time.Value = value;
        }

        public Player? PreviousPlayer {
            get => _previousPlayer.Value;
            set => _previousPlayer.Value = value;
        }

        public Action? OnBack { get; set; }

        #endregion

        #region Data

        private readonly record struct ClanTag(string Text, Color Color);

        private readonly record struct SocialLink(string Name, Color Color, string Link);

        private static IReadOnlyList<ClanTag> CreateClans(Player? player) {
            return player?.clans?.Select(static x => new ClanTag(x.tag, ProfileStyle.ParseColor(x.color))).ToArray() ?? [];
        }

        private static IReadOnlyList<SocialLink> CreateSocials(Player? player) {
            if (player == null) {
                return [];
            }

            var links = new List<SocialLink>();

            if (player.id.StartsWith("7656119")) {
                links.Add(new("Steam", new(0.4f, 0.6f, 0.85f), $"https://steamcommunity.com/profiles/{player.id}"));
            }

            if (player.socials != null) {
                links.AddRange(player.socials.Select(static x => new SocialLink(x.service, GetSocialColor(x.service), x.link)));
            }

            return links;
        }

        private static Color GetSocialColor(string service) {
            return service switch {
                "Twitch" => new(0.57f, 0.27f, 1f),
                "YouTube" => new(1f, 0.2f, 0.2f),
                "Twitter" => new(0.11f, 0.63f, 0.95f),
                "Discord" => new(0.35f, 0.4f, 0.95f),
                "BeatSaver" => new(0.6f, 0.4f, 1f),
                _ => ProfileStyle.NeutralColor
            };
        }

        #endregion

        #region Construct

        private State<Player?> _player = null!;
        private State<PlayerFollowersInfo?> _followers = null!;
        private State<float> _time = null!;
        private State<Player?> _previousPlayer = null!;

        private const float BackButtonHeight = 3.4f;

        protected override GameObject Construct() {
            _player = Remember<Player?>(null);
            _followers = Remember<PlayerFollowersInfo?>(null);
            _time = Remember(0f);
            _previousPlayer = Remember<Player?>(null);

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column
                },

                FlexItem = {
                    Size = new() { y = 32f },
                    sPosition = ProfileReveal.Slide(ProfileReveal.Stage(_time, 0f))
                },

                Children = {
                    new ProfileCover {
                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = 0f
                        },

                        sCoverUrl = _player.Map(static x => x?.profileSettings?.profileCover),
                        sFallbackUrl = _player.Map(static x => x?.avatar)
                    },

                    CreateIdentity(),
                    CreateStrip(),
                    CreateBackButton()
                }
            }.With(static x => x.Content.AddComponent<UnityEngine.UI.RectMask2D>()).Use();
        }

        [StateGen]
        private Layout CreateIdentity() {
            var avatarStage = ProfileReveal.Stage(_time, 0.1f, 0.8f);
            var nameStage = ProfileReveal.Stage(_time, 0.18f);
            var clansStage = ProfileReveal.Stage(_time, 0.28f);

            return new Layout {
                FlexController = {
                    AlignItems = Align.Center,
                    Padding = new() { left = 2.5f, right = 3f },
                    Gap = 3f
                },

                FlexItem = {
                    FlexGrow = 1f
                },

                Children = {
                    new ReeWrapperV2<PlayerAvatar> {
                        FlexItem = {
                            AlignSelf = Align.Stretch,
                            AspectRatio = 1f
                        },

                        ContentTransform = {
                            slocalScale = ProfileReveal.Pop(avatarStage, 0.4f)
                        }
                    }.On(_player, static (x, y) => x.ReeComponent.SetAvatar(y)).WithAlpha(avatarStage),

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            AlignItems = Align.FlexStart,
                            Gap = 0.5f
                        },

                        FlexItem = {
                            FlexGrow = 1f,
                            FlexShrink = 1f,
                            AlignSelf = Align.Stretch
                        },

                        Children = {
                            new Label {
                                FlexItem = {
                                    MaxSize = new() { x = 100.pct },
                                    sPosition = nameStage.Map(static x => new YogaFrame { left = (1f - x) * -4f })
                                },

                                FontSize = 7f,
                                FontStyle = FontStyles.Bold,
                                EnableWrapping = false,
                                Overflow = TextOverflowModes.Ellipsis,
                                RichText = false,
                                sText = _player.Map(static x => x?.name ?? "")
                            }.WithAlpha(nameStage),

                            new Repeater<ClanTag, ProfileTag> {
                                FlexController = {
                                    FlexDirection = FlexDirection.Row,
                                    AlignItems = Align.Center,
                                    Gap = 1f
                                },

                                FlexItem = {
                                    Margin = new() { top = -1.5f }
                                },

                                SelectionMode = SelectionMode.None,
                                sItems = _player.Map(CreateClans),

                                ConstructCell = ctx => {
                                    var appear = RememberDerived(ctx, clansStage, static (cell, x) => ProfileReveal.Progress(x, cell.Index * ProfileReveal.ItemDelay * 2f));

                                    return new ProfileTag {
                                        FontSize = 2.8f,
                                        sText = ctx.Map(static x => x.Item.Text),
                                        sColor = ctx.Map(static x => x.Item.Color),
                                        sAppear = appear
                                    };
                                }
                            },

                            new Label {
                                FlexItem = {
                                    MaxSize = new() { x = 100.pct },
                                    Margin = new() { top = 0.5f }
                                },

                                FontSize = 2.6f,
                                FontStyle = FontStyles.Italic,
                                EnableWrapping = false,
                                Overflow = TextOverflowModes.Ellipsis,
                                Color = BeatSaberStyle.SecondaryTextColor,
                                sText = _player.Map(static x => x?.profileSettings?.message ?? ""),
                                sEnabled = _player.Map(static x => !string.IsNullOrEmpty(x?.profileSettings?.message))
                            }.WithAlpha(clansStage)
                        }
                    }
                }
            };
        }

        [StateGen]
        private BsButton CreateBackButton() {
            return new BsButton {
                FlexItem = {
                    PositionType = PositionType.Absolute,
                    Position = new() { left = 0.6f, top = 0.6f },
                    Size = new() { y = BackButtonHeight }
                },

                FlexController = {
                    AlignItems = Align.Center,
                    Padding = new() { left = 1f, right = 1f },
                    Gap = 0.8f
                },

                ShowUnderline = false,
                Skew = 0f,
                sEnabled = _previousPlayer.Map(static x => x != null),
                OnClick = () => OnBack?.Invoke(),

                ConstructContent = (color, _) => new Layout {
                    FlexController = {
                        AlignItems = Align.Center,
                        Gap = 0.8f
                    },

                    Children = {
                        new Image {
                            FlexItem = {
                                Size = 2f
                            },

                            ContentTransform = {
                                localEulerAngles = new(0f, 0f, -90f)
                            },

                            Sprite = GameResources.Sprites.ArrowIcon,
                            PreserveAspect = true,
                            sColor = color.In()
                        },

                        new Label {
                            FontSize = 2.6f,
                            Alignment = TextAlignmentOptions.Top,
                            Text = "Back",
                            sColor = color.In()
                        }
                    }
                }
            };
        }

        [StateGen]
        private Background CreateStrip() {
            var stage = ProfileReveal.Stage(_time, 0.36f);

            return new Background {
                FlexController = {
                    AlignItems = Align.Center,
                    Padding = new() { left = 2.5f, right = 2f },
                    Gap = 4f
                },

                FlexItem = {
                    Size = new() { y = 6f }
                },

                Children = {
                    new Repeater<SocialLink, ProfileTag> {
                        FlexController = {
                            FlexDirection = FlexDirection.Row,
                            AlignItems = Align.Center,
                            Gap = 1f
                        },

                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = new() { right = 2f, top = 0f, bottom = 0f }
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = _player.Map(CreateSocials),

                        ConstructCell = ctx => {
                            var appear = RememberDerived(ctx, stage, static (cell, x) => ProfileReveal.Progress(x, cell.Index * ProfileReveal.ItemDelay * 2f));
                            ProfileTag tag = null!;

                            return tag = new ProfileTag {
                                FontSize = 2.4f,
                                sText = ctx.Map(static x => x.Item.Name),
                                sColor = ctx.Map(static x => x.Item.Color),
                                sAppear = appear,
                                OnClick = () => {
                                    EnvironmentUtils.OpenBrowserPage(ctx.Item.Link);
                                    ActionBubble.Info(tag.ContentTransform, ProfileStyle.OpenedInBrowserText);
                                }
                            };
                        }
                    },

                    new ProfileFollowers {
                        sPlayerId = _player.Map(static x => x?.id),
                        sInfo = _followers
                    }
                }
            }.AsBlurBackground().WithAlpha(stage);
        }

        #endregion
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
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
    internal class ProfileRankings : ReactiveComponent {
        #region Props

        public Player? Player {
            get => _player.Value;
            set => _player.Value = value;
        }

        public float Progress {
            get => _progress.Value;
            set => _progress.Value = value;
        }

        #endregion

        #region Loading

        private Player? _loadedPlayer;

        private void Load(Player player) {
            _ = LoadAsync(player).RunCatching();
        }

        private async Task LoadAsync(Player player) {
            _loadedPlayer = player;
            _rankings.Value = null;

            var result = await MiniRankingsRequest.Send(player.rank, player.country, player.countryRank).Join();

            if (player != _loadedPlayer) {
                return;
            }

            if (result.Result == null) {
                _loadedPlayer = null;
            }

            _rankings.Value = result.Result;
        }

        #endregion

        #region Construct

        private const float RowHeight = 3.6f;
        private const float MaxRowHeight = 5f;

        private State<Player?> _player = null!;
        private State<float> _progress = null!;
        private State<MiniRankings?> _rankings = null!;

        protected override GameObject Construct() {
            _player = Remember<Player?>(null);
            _progress = Remember(0f);
            _rankings = Remember<MiniRankings?>(null);

            var requested = RememberDerived(_player, _progress, static (x, y) => y > 0f ? x : null);
            var shown = _progress.Map(static x => x > 0.001f);
            var loading = _rankings.Map(static x => x == null);
            var listAlpha = RememberAnimated(loading.Map(static x => x ? 0f : 1f), 200.ms);
            var globalIcon = Remember(GameResources.Sprites.GlobalIcon);
            var countryIcon = _player.Map(static x => GetFlag(x?.country));

            requested.AddCallback(this, static (x, y) => {
                if (y != null && y != x._loadedPlayer) {
                    x.Load(y);
                }
            });

            return new Layout {
                FlexController = {
                    Gap = 1.5f
                },

                FlexItem = {
                    PositionType = PositionType.Absolute,
                    Size = new() { x = 100.pct },
                    sPosition = _progress.Map(static x => new YogaFrame { left = ((x - 1f) * 100f).pct, top = 0f, bottom = 0f })
                },

                sEnabled = shown,

                Children = {
                    CreateColumn(
                        globalIcon,
                        "Global ranking",
                        _rankings.Map(static x => (IReadOnlyList<RankingPlayer>)(x?.global ?? [])),
                        false,
                        loading,
                        listAlpha
                    ),

                    CreateColumn(
                        countryIcon,
                        "Country ranking",
                        _rankings.Map(static x => (IReadOnlyList<RankingPlayer>)(x?.country ?? [])),
                        true,
                        loading,
                        listAlpha
                    )
                }
            }.Use();
        }

        private static Sprite GetFlag(string? country) {
            return BundleLoader.GetSpriteFromBundle(country?.ToLower() ?? "") ?? BundleLoader.LocationIcon;
        }

        [StateGen]
        private Background CreateColumn(IState<Sprite> icon, string title, IState<IReadOnlyList<RankingPlayer>> players, bool country, IState<bool> loading, IState<float> alpha) {
            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Padding = 1.5f,
                    Gap = 0.8f
                },

                FlexItem = {
                    FlexGrow = 1f,
                    FlexBasis = 0f
                },

                Children = {
                    new Layout {
                        FlexController = {
                            AlignItems = Align.Center,
                            Gap = 1f
                        },

                        FlexItem = {
                            Size = new() { y = 3f },
                            Margin = new() { bottom = 0.8f }
                        },

                        Children = {
                            new Image {
                                FlexItem = {
                                    Size = new() { x = 3f, y = 2.4f }
                                },

                                PreserveAspect = true,
                                sSprite = icon.In()
                            },

                            new Label {
                                FontSize = 2.8f,
                                FontStyle = FontStyles.Bold,
                                Alignment = TextAlignmentOptions.CaplineLeft,
                                Text = title
                            }
                        }
                    },

                    new Repeater<RankingPlayer, Background> {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            Gap = 0.5f
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = players.In(),

                        ConstructCell = ctx => {
                            var isSelf = RememberDerived(ctx, _player, static (x, y) => x.Item.id == y?.id);
                            var rankText = ctx.Map(x => FormatRank(country ? x.Item.countryRank : x.Item.rank));
                            var diffText = RememberDerived(ctx, _player, static (x, y) => FormatDiff(x.Item, y));
                            var graphicState = Remember(GraphicState.None);

                            return new Background {
                                FlexController = {
                                    AlignItems = Align.Center,
                                    Padding = new() { left = 1f, right = 1f },
                                    Gap = 0.7f
                                },

                                FlexItem = {
                                    FlexGrow = 1f,
                                    FlexBasis = 0f,
                                    MinSize = new() { y = RowHeight },
                                    MaxSize = new() { y = MaxRowHeight }
                                },

                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 10f,
                                sColor = graphicState.MapColorSet(BeatSaberStyle.BsSegmentedControlColors.CellBackgroundColors),

                                Children = {
                                    new Label {
                                        FontSize = 2.2f,
                                        FontStyle = FontStyles.Bold,
                                        Alignment = TextAlignmentOptions.CaplineLeft,
                                        EnableWrapping = false,
                                        sText = rankText
                                    },

                                    new Image {
                                        FlexItem = {
                                            Size = new() { x = 2.6f, y = 2f }
                                        },

                                        PreserveAspect = true,
                                        Enabled = !country,
                                        sSprite = ctx.Map(static x => GetFlag(x.Item.country))
                                    },

                                    new Label {
                                        FlexItem = {
                                            FlexGrow = 1f,
                                            FlexShrink = 1f
                                        },

                                        FontSize = 2.2f,
                                        Alignment = TextAlignmentOptions.CaplineLeft,
                                        EnableWrapping = false,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        RichText = false,
                                        sFontStyle = isSelf.Map(static x => x ? FontStyles.Bold : FontStyles.Normal),
                                        sText = ctx.Map(static x => x.Item.name)
                                    },

                                    new Label {
                                        FontSize = 2.2f,
                                        Alignment = TextAlignmentOptions.CaplineRight,
                                        EnableWrapping = false,
                                        Color = Color.Lerp(ProfileStyle.PpColor, Color.white, 0.35f),
                                        sText = ctx.Map(static x => $"{x.Item.pp.ToString("N2", FormatUtils.ScoreFormatInfo)}pp")
                                    },

                                    new Label {
                                        FlexItem = {
                                            MinSize = new() { x = 4.8f }
                                        },

                                        FontSize = 1.8f,
                                        Alignment = TextAlignmentOptions.CaplineRight,
                                        EnableWrapping = false,
                                        sText = diffText
                                    }
                                }
                            }.WithPointerEvents(
                                onEnter: _ => graphicState.IsHovered = !isSelf.Value,
                                onLeave: _ => graphicState.IsHovered = false,
                                onDown: _ => {
                                    if (isSelf.Value) return;
                                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                    PlayerProfileFlowCoordinator.Open(ctx.Item.id);
                                }
                            );
                        }
                    }.WithAlpha(alpha),

                    new Spinner {
                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Size = 4f,
                            Position = new() { left = 50.pct, top = 50.pct },
                            Margin = new() { left = -2f, top = -2f }
                        },

                        sEnabled = loading.In()
                    }
                }
            }.AsBlurBackground();
        }

        private static string FormatRank(int rank) {
            return $"#{rank.ToString("N0", FormatUtils.ScoreFormatInfo)}";
        }

        private static string FormatDiff(RankingPlayer player, Player? self) {
            if (self == null || player.id == self.id) {
                return "";
            }

            var diff = player.pp - self.pp;

            return diff switch {
                > 0f => $"<color=#6CFF6C>+{diff:N2}</color>",
                < 0f => $"<color=#FF6C6C>{diff:N2}</color>",
                _ => ""
            };
        }

        #endregion
    }
}
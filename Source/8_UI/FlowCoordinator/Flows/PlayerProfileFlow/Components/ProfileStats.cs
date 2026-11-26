using System;
using System.Collections.Generic;
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
    internal class ProfileStats : ReactiveComponent {
        #region Props

        public Player? Player {
            get => _player.Value;
            set => _player.Value = value;
        }

        public float Time {
            get => _time.Value;
            set => _time.Value = value;
        }

        #endregion

        #region Data

        private const float ItemsDelay = 0.55f;

        private readonly record struct InfoRow(string Title, string Value, Sprite? Icon = null);

        private readonly record struct StatChip(string Title, double Value, Func<double, string> Format, Color Color);

        private static string FormatCount(double value) => ((long)Math.Round(value)).ToString("N0", FormatUtils.ScoreFormatInfo);
        private static string FormatPercent(double value) => $"{value * 100d:F2}%";
        private static string FormatPlace(double value) => $"#{value:F2}";
        private static string FormatPp(double value) => $"{value:F2}pp";

        private static IReadOnlyList<InfoRow> CreateInfo(Player? player) {
            if (player == null) {
                return [];
            }

            var change = player.lastWeekRank > 0 ? player.lastWeekRank - player.rank : 0;
            var changeText = change switch {
                > 0 => $" <color=#6CFF6C><size=75%>+{change}</size></color>",
                < 0 => $" <color=#FF6C6C><size=75%>{change}</size></color>",
                _ => ""
            };

            var headset = player.scoreStats is { } stats ? ReplayUtils.HMDToName((ReplayUtils.HMD)stats.topHMD) : null;
            var flag = BundleLoader.GetSpriteFromBundle(player.country.ToLower()) ?? BundleLoader.LocationIcon;

            return [
                new("Global", $"#{player.rank}{changeText}"),
                new("Country", $"#{player.countryRank}", flag),
                new("Headset", headset ?? "Unknown"),
                new("Platform", FormatUtils.GetFullPlatformName(player.scoreStats?.topPlatform ?? player.platform ?? ""))
            ];
        }

        private static IReadOnlyList<StatChip> CreateChips(Player? player) {
            if (player?.scoreStats is not { } stats) {
                return [];
            }

            var neutral = ProfileStyle.NeutralColor;
            var acc = ProfileStyle.AccColor;
            var rank = ProfileStyle.RankColor;

            return [
                new("Total play count", stats.totalPlayCount, FormatCount, neutral),
                new("Total score", stats.totalScore, FormatCount, neutral),
                new("Ranked play count", stats.rankedPlayCount, FormatCount, neutral),
                new("Total ranked score", stats.totalRankedScore, FormatCount, neutral),
                new("Top PP", stats.topPp, FormatPp, ProfileStyle.PpColor),
                new("Best acc", stats.topAccuracy, FormatPercent, acc),
                new("Average acc", stats.averageAccuracy, FormatPercent, acc),
                new("Median acc", stats.medianAccuracy, FormatPercent, acc),
                new("Average ranked acc", stats.averageRankedAccuracy, FormatPercent, acc),
                new("Weighted ranked acc", stats.averageWeightedRankedAccuracy, FormatPercent, acc),
                new("Median ranked acc", stats.medianRankedAccuracy, FormatPercent, acc),
                new("Weighted average rank", stats.averageWeightedRankedRank, FormatPlace, rank),
                new("Average rank", stats.averageRank, FormatPlace, rank),
                new("SS+", stats.sspPlays, FormatCount, new(1f, 0.85f, 0.3f)),
                new("SS", stats.ssPlays, FormatCount, new(1f, 0.9f, 0.5f)),
                new("S+", stats.spPlays, FormatCount, new(0.6f, 0.96f, 0.6f)),
                new("S", stats.sPlays, FormatCount, new(0.44f, 0.86f, 0.44f)),
                new("A", stats.aPlays, FormatCount, new(0.5f, 0.77f, 1f))
            ];
        }

        #endregion

        #region Construct

        private State<Player?> _player = null!;
        private State<float> _time = null!;

        protected override GameObject Construct() {
            _player = Remember<Player?>(null);
            _time = Remember(0f);

            var stage = ProfileReveal.Stage(_time, 0.5f);

            return new Layout {
                FlexController = {
                    Gap = 1.5f
                },

                FlexItem = {
                    sPosition = ProfileReveal.Slide(stage)
                },

                Children = {
                    CreateInfoBlock(),
                    CreateChipsBlock()
                }
            }.WithAlpha(stage).Use();
        }

        [StateGen]
        private Background CreateInfoBlock() {
            var ppStage = ProfileReveal.Stage(_time, ItemsDelay, 1.4f);
            var ppText = RememberDerived(_player, ppStage, static (x, y) => $"{(x?.pp ?? 0f) * y:N2}pp");

            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Padding = 2f,
                    Gap = 1f
                },

                FlexItem = {
                    Size = new() { x = 34f },
                    AlignSelf = Align.FlexStart
                },

                Children = {
                    new ProfileTag {
                        FlexItem = {
                            AlignSelf = Align.FlexStart
                        },

                        FontSize = 4.2f,
                        Color = ProfileStyle.PpColor,
                        Interactable = false,
                        sText = ppText
                    },

                    new Repeater<InfoRow, Layout> {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            Gap = 0.4f
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = _player.Map(CreateInfo),

                        ConstructCell = ctx => {
                            var alpha = RememberDerived(ctx, _time, static (cell, x) => ProfileReveal.Progress(x, ItemsDelay + cell.Index * ProfileReveal.ItemDelay * 2f));

                            return new Layout {
                                FlexController = {
                                    JustifyContent = Justify.SpaceBetween,
                                    AlignItems = Align.Center
                                },

                                FlexItem = {
                                    Size = new() { y = 3.6f }
                                },

                                Children = {
                                    new Label {
                                        FontSize = 2.4f,
                                        FontStyle = FontStyles.Bold,
                                        Color = BeatSaberStyle.SecondaryTextColor,
                                        sText = ctx.Map(static x => x.Item.Title)
                                    },

                                    new Layout {
                                        FlexController = {
                                            AlignItems = Align.Center,
                                            Gap = 0.8f
                                        },

                                        Children = {
                                            new Label {
                                                FontSize = 2.8f,
                                                sText = ctx.Map(static x => x.Item.Value)
                                            },

                                            new Image {
                                                FlexItem = {
                                                    Size = new() { x = 3.4f, y = 2.6f }
                                                },

                                                PreserveAspect = true,
                                                sSprite = ctx.Map(static x => x.Item.Icon),
                                                sEnabled = ctx.Map(static x => x.Item.Icon != null)
                                            }
                                        }
                                    }
                                }
                            }.WithAlpha(alpha);
                        }
                    }
                }
            }.AsBlurBackground();
        }

        [StateGen]
        private Background CreateChipsBlock() {
            return new Background {
                FlexController = {
                    Padding = 2f
                },

                FlexItem = {
                    FlexGrow = 1f,
                    FlexShrink = 1f
                },

                Children = {
                    new Repeater<StatChip, ProfileTag> {
                        FlexController = {
                            FlexDirection = FlexDirection.Row,
                            FlexWrap = Wrap.Wrap,
                            AlignContent = Align.FlexStart,
                            Gap = 1f
                        },

                        FlexItem = {
                            FlexGrow = 1f,
                            FlexShrink = 1f
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = _player.Map(CreateChips),

                        ConstructCell = ctx => {
                            var appear = RememberDerived(ctx, _time, static (cell, x) => ProfileReveal.Progress(x, ItemsDelay + cell.Index * 0.05f, 0.45f));
                            var count = RememberDerived(ctx, _time, static (cell, x) => ProfileReveal.Progress(x, ItemsDelay + cell.Index * ProfileReveal.ItemDelay, 1.3f));
                            var text = RememberDerived(ctx, count, static (cell, x) => $"{cell.Item.Title}  <color=#FFFFFF60>|</color>  {cell.Item.Format(cell.Item.Value * x)}");

                            return new ProfileTag {
                                FontSize = 2.5f,
                                sColor = ctx.Map(static x => x.Item.Color),
                                sAppear = appear,
                                sText = text
                            };
                        }
                    }
                }
            }.AsBlurBackground();
        }

        #endregion
    }
}

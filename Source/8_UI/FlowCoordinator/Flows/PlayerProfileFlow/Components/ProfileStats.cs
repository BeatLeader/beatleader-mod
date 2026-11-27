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
using AnimationCurve = Reactive.AnimationCurve;
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
        private AnimatedState<float> _detailsProgress = null!;

        protected override GameObject Construct() {
            _player = Remember<Player?>(null);
            _time = Remember(0f);
            _detailsProgress = RememberAnimated(0f, 400.ms, AnimationCurve.EaseOutQuint);

            var stage = ProfileReveal.Stage(_time, 0.5f);

            _player.AddCallback(this, static (x, _) => x._detailsProgress.SetValueImmediate(0f));

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
            var buttonHover = RememberAnimated(1f, 300.ms, AnimationCurve.EaseOutBack);
            var buttonScale = buttonHover.Map(static x => Vector3.one * x);
            var arrowRotation = _detailsProgress.Map(static x => new Vector3(0f, 0f, 90f + 180f * x));
            var contentColor = Color.Lerp(ProfileStyle.PpColor, Color.white, 0.6f);

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
                    new Background {
                        FlexController = {
                            AlignItems = Align.Center,
                            Padding = new() { left = 1.2f, right = 0.8f, top = 0.2f, bottom = 0.2f },
                            Gap = 1f
                        },

                        FlexItem = {
                            AlignSelf = Align.FlexStart
                        },

                        ContentTransform = {
                            slocalScale = buttonScale
                        },

                        PixelsPerUnit = 10f,
                        Sprite = BundleLoader.Sprites.background,
                        Material = global::Reactive.BeatSaber.GameResources.UINoGlowMaterial,
                        Color = Color.Lerp(ProfileStyle.PpColor, Color.black, 0.55f),

                        Children = {
                            new Label {
                                FontSize = 4.2f,
                                FontStyle = FontStyles.Bold,
                                Alignment = TextAlignmentOptions.Capline,
                                EnableWrapping = false,
                                Color = contentColor,
                                sText = ppText
                            },

                            new Image {
                                FlexItem = {
                                    Size = new() { x = 0.5f, y = 3.4f }
                                },

                                Sprite = GameResources.Sprites.VerticalRoundRect,
                                ImageType = UnityEngine.UI.Image.Type.Sliced,
                                PixelsPerUnit = 32f,
                                Color = contentColor.ColorWithAlpha(0.6f)
                            },

                            new Image {
                                FlexItem = {
                                    Size = 3f,
                                    Margin = new() { left = -0.3f }
                                },

                                ContentTransform = {
                                    slocalEulerAngles = arrowRotation
                                },

                                Sprite = GameResources.Sprites.ArrowIcon,
                                Color = contentColor,
                                PreserveAspect = true
                            }
                        }
                    }.WithPointerEvents(
                        onEnter: _ => buttonHover.TargetValue = 1.06f,
                        onLeave: _ => buttonHover.TargetValue = 1f,
                        onDown: _ => {
                            global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                            _detailsProgress.TargetValue = _detailsProgress.TargetValue > 0.5f ? 0f : 1f;
                        }
                    ),

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
        private Layout CreateChipsBlock() {
            var chipsAlpha = _detailsProgress.Map(static x => 1f - x);
            var chipsOffset = _detailsProgress.Map(static x => new YogaFrame { left = (x * 30f).pct });
            var chipsScale = _detailsProgress.Map(static x => Vector3.one * Mathf.Lerp(1f, 0.75f, x));

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Row
                },

                FlexItem = {
                    FlexGrow = 1f,
                    FlexShrink = 1f
                },

                Children = {
                    new Background {
                        FlexController = {
                            Padding = 2f
                        },

                        FlexItem = {
                            FlexGrow = 1f,
                            sPosition = chipsOffset
                        },

                        ContentTransform = {
                            pivot = new(1f, 0.5f),
                            slocalScale = chipsScale
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
                    }.AsBlurBackground().WithAlpha(chipsAlpha),

                    new ProfileRankings {
                        sPlayer = _player,
                        sProgress = _detailsProgress
                    }
                }
            }.AsRectMask();
        }

        #endregion
    }
}

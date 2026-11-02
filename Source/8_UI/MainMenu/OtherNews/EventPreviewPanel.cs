using System;
using System.Collections;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EventPreviewPanel : ReactiveComponent {
        public EventPreviewPanel(CellContext<PlatformEvent> context) : base(false) {
            _context = context;
            ConstructAndInit();
        }

        public Action<PlatformEvent>? OnClick { get; set; }

        private readonly CellContext<PlatformEvent> _context;
        private State<string> _bottomText = null!;

        protected override GameObject Construct() {
            _bottomText = Remember(string.Empty);
            var graphicState = Remember(GraphicState.None);

            var colors = new ColorSet {
                Color = Color.clear,
                HoveredColor = new Color(0, 0, 0, 0.5f + 0.4f)
            };

            return new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Row,
                        Gap = 1f,
                        Padding = new() { left = 1.5f, top = 1f, right = 2f, bottom = 1f },
                        AlignItems = Align.Center
                    },

                    FlexItem = {
                        Margin = new() { left = 1f, right = 1f }
                    },

                    Sprite = BeatSaberResources.Sprites.background,
                    PixelsPerUnit = 10,
                    Skew = UIStyle.Skew,
                    sColor = graphicState.MapColorSet(colors),

                    Children = {
                        new WebImage {
                            FlexItem = {
                                Size = new() { x = 10, y = 10 }
                            },

                            Sprite = BundleLoader.UnknownIcon,
                            Material = BundleLoader.RoundTextureMaterial,
                            Skew = UIStyle.Skew,
                            sSrc = _context.Map(x => x.Item.image)
                        },

                        new Layout {
                            FlexController = {
                                FlexDirection = FlexDirection.Column
                            },

                            FlexItem = {
                                Margin = new() { left = 1 },
                                FlexGrow = 1,
                                FlexShrink = 1
                            },

                            Children = {
                                new Label {
                                    FontSize = 4f,
                                    FontStyle = FontStyles.Italic,
                                    Overflow = TextOverflowModes.Ellipsis,
                                    Alignment = TextAlignmentOptions.CaplineLeft,
                                    sText = _context.Map(x => x.Item.name)
                                },

                                new Label {
                                    FlexItem = {
                                        Margin = new() { left = -0.6f }
                                    },

                                    FontSize = 3.5f,
                                    FontStyle = FontStyles.Italic,
                                    Overflow = TextOverflowModes.Ellipsis,
                                    Alignment = TextAlignmentOptions.TopLeft,
                                    RichText = true, // For color tags
                                    sText = _bottomText
                                }
                            }
                        },

                        new BsTextButton {
                            FlexItem = {
                                Size = new() { x = 12, y = 8 }
                            },

                            Text = "Details",
                            OnClick = () => OnClick?.Invoke(_context.Item)
                        }
                    }
                }
                .WithPointerEvents(
                    onEnter: _ => graphicState.IsHovered = true,
                    onLeave: _ => graphicState.IsHovered = false,
                    onDown: _ => {
                        global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                        OnClick?.Invoke(_context.Item);
                    }
                )
                .On(_context, (_, ctx) => UpdateBottomText(ctx.Item), lazy: true)
                .Use();
        }

        #region Timer Logic

        private void UpdateBottomText(PlatformEvent item) {
            StopAllCoroutines();

            var timeSpan = FormatUtils.GetRelativeTime(item.endDate);
            var remainingTime = timeSpan;

            if (timeSpan < TimeSpan.Zero) {
                _bottomText.Value = $"<color=#88FF88>{FormatRemainingTime(-timeSpan)}";
                remainingTime = -timeSpan;
            } else {
                var date = FormatUtils.GetRelativeTimeString(timeSpan, false);
                _bottomText.Value = $"<color=#884444>Ended {date}";
                return; // No need to schedule update if it has ended
            }

            TimeSpan updateDelay;
            if (remainingTime.TotalDays >= 1) {
                updateDelay = TimeSpan.FromDays(Math.Ceiling(remainingTime.TotalDays)) - remainingTime;
            } else if (remainingTime.TotalHours >= 1) {
                updateDelay = TimeSpan.FromHours(Math.Ceiling(remainingTime.TotalHours)) - remainingTime;
            } else if (remainingTime.TotalMinutes >= 1) {
                updateDelay = TimeSpan.FromMinutes(Math.Ceiling(remainingTime.TotalMinutes)) - remainingTime;
            } else {
                updateDelay = TimeSpan.FromSeconds(Math.Ceiling(remainingTime.TotalSeconds)) - remainingTime;
                if (updateDelay <= TimeSpan.Zero) updateDelay = TimeSpan.FromSeconds(1);
            }

            StartCoroutine(UpdateAfterDelay(updateDelay, item));
        }

        private IEnumerator UpdateAfterDelay(TimeSpan delay, PlatformEvent item) {
            yield return new WaitForSeconds((float)delay.TotalSeconds);
            UpdateBottomText(item);
        }

        private static string FormatRemainingTime(TimeSpan span) {
            if (span.TotalDays >= 1) {
                return $"Ongoing! {(int)span.TotalDays} day{((int)span.TotalDays > 1 ? "s" : "")} left";
            }
            if (span.TotalHours >= 1) {
                return $"Ongoing! {(int)span.TotalHours} hour{((int)span.TotalHours > 1 ? "s" : "")} left";
            }
            if (span.TotalMinutes >= 1) {
                return $"Ongoing! {(int)span.TotalMinutes} minute{((int)span.TotalMinutes > 1 ? "s" : "")} left";
            }
            return $"Ongoing! {(int)span.TotalSeconds} second{((int)span.TotalSeconds > 1 ? "s" : "")} left";
        }

        #endregion
    }
}
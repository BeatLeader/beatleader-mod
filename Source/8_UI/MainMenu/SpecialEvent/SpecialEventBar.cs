using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AnimationCurve = Reactive.AnimationCurve;
using Image = Reactive.BeatSaber.Components.Image;

namespace BeatLeader.UI.MainMenu;

internal class SpecialEventBar : ReactiveComponent {
    #region Public API

    public Action? OnPlayClick { get; set; }
    public Action? OnBackClick { get; set; }
    public Action<PlatformEventMap>? OnDayChanged { get; set; }

    public void SetData(PlatformEventStatus status) {
        _event.Value = status;
        _event.EnforceInvalidation();
    }

    #endregion

    #region Construct

    private State<PlatformEventStatus> _event = null!;
    private ReactiveComponent _bar = null!;

    protected override GameObject Construct() {
        _event = Remember<PlatformEventStatus>(null!);

        var initialCalendarButtonWidth = 8f;

        var calendarOpened = Remember(false);
        var calendarHeight = RememberAnimated(0f, 250.ms, AnimationCurve.EaseOutExpo);
        var calendarButtonWidth = RememberAnimated(initialCalendarButtonWidth, 150.ms, AnimationCurve.EaseOutExpo);

        var itemsAlpha = RememberAnimated(1f, 150.ms, AnimationCurve.EaseOutExpo);
        var bgAlpha = RememberAnimated(0f, 150.ms, AnimationCurve.EaseInOut);

        var content = new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Padding = 1f
                },

                FlexItem = {
                    MinSize = 100.pct,
                    Size = new() { x = 100.pct },
                    Position = new() { bottom = 0.pt },
                    PositionType = PositionType.Absolute
                },

                Children = {
                    new Image {
                        FlexItem = {
                            Position = 0.pt,
                            PositionType = PositionType.Absolute
                        },

                        sColor = bgAlpha.Map(static x => Color.white.ColorWithAlpha(x))
                    }.AsBlurBackground(pixelsPerUnit: 10f),

                    // Bar
                    new Layout {
                        FlexController = {
                            JustifyContent = Justify.Center,
                            AlignItems = Align.Stretch,
                            Gap = 2.pt
                        },

                        FlexItem = {
                            Size = new() { x = 100.pct, y = 8.pt }
                        },

                        Children = {
                            new BsTextButton {
                                    FlexItem = {
                                        Size = new() { y = 90.pct, x = 8f },
                                        Position = new() { left = 0.pt },
                                        PositionType = PositionType.Absolute,
                                        AlignSelf = Align.Center
                                    },

                                    Text = "←",
                                    ShowUnderline = false,
                                    Skew = 0f,
                                    OnClick = () => OnBackClick?.Invoke()
                                }
                                .WithNativeComponent(out CanvasGroup backButtonGroup)
                                .On(itemsAlpha, (_, y) => backButtonGroup.alpha = y, lazy: true),

                            new BsTextButton {
                                    Text = " Play ",
                                    OnClick = () => OnPlayClick?.Invoke()
                                }
                                .WithNativeComponent(out CanvasGroup playButtonGroup)
                                .On(itemsAlpha, (_, y) => playButtonGroup.alpha = y, lazy: true),

                            // Calendar button
                            new Layout {
                                FlexItem = {
                                    Size = new() { x = initialCalendarButtonWidth.pt, y = 90.pct },
                                    Position = new() { left = 0.pt },
                                    PositionType = PositionType.Absolute,
                                    AlignSelf = Align.Center
                                },

                                Children = {
                                    new BsTextButton {
                                        FlexItem = {
                                            Position = new() { left = 0.pt },
                                            PositionType = PositionType.Absolute,
                                            sSize = calendarButtonWidth.Map(x => new YogaVector { x = x.pt, y = 100.pct })
                                        },

                                        Text = "💿",
                                        ShowUnderline = false,
                                        Skew = 0f,
                                        sEnabled = calendarOpened,

                                        OnClick = () => {
                                            ReeModalSystem.OpenModal<EventDetailsDialog>(ContentTransform, _event.Value.eventDescription);
                                        }
                                    }
                                }
                            }.AsFlexGroup(),

                            new Layout {
                                FlexItem = {
                                    Size = new() { x = initialCalendarButtonWidth.pt, y = 90.pct },
                                    Position = new() { right = 0.pt },
                                    PositionType = PositionType.Absolute,
                                    AlignSelf = Align.Center
                                },

                                Children = {
                                    new BsTextButton {
                                        FlexItem = {
                                            Position = new() { right = 0.pt },
                                            PositionType = PositionType.Absolute,
                                            sSize = calendarButtonWidth.Map(x => new YogaVector { x = x.pt, y = 100.pct })
                                        },

                                        sText = calendarOpened.Map(x => x ? "❌" : "📅"),
                                        ShowUnderline = false,
                                        Skew = 0f,

                                        OnClick = () => {
                                            calendarOpened.Value = !calendarOpened;
                                            calendarButtonWidth.TargetValue = calendarOpened ? _bar.ContentTransform.rect.width / 2f - 2f : initialCalendarButtonWidth;

                                            if (!calendarOpened) {
                                                calendarHeight.TargetValue = 0f;
                                            }

                                            itemsAlpha.TargetValue = calendarOpened ? 0f : 1f;
                                            bgAlpha.TargetValue = calendarOpened ? 1f : 0f;
                                        }
                                    }
                                }
                            }.AsFlexGroup()
                        }
                    },

                    // Calendar
                    new Layout {
                            FlexItem = {
                                sSize = calendarHeight.Map(x => new YogaVector { y = x.pt }).InLazy()
                            },

                            WithinLayoutIfDisabled = true,
                            Children = {
                                new EventCalendar {
                                        FlexItem = {
                                            Position = new() { left = 0.pt, top = 0.pt, right = 0.pt },
                                            PositionType = PositionType.Absolute,
                                            Margin = new() { top = 1.pt, bottom = 2.pt }
                                        },

                                        OnDayChanged = x => OnDayChanged?.Invoke(x),
                                    }
                                    .On(_event, (x, y) => x.SetData(y), lazy: true)
                                    .With(
                                        x => x.LayoutAppliedEvent += _ => {
                                            if (calendarOpened) {
                                                calendarHeight.TargetValue = x.ContentTransform.rect.height + 3;
                                            }
                                        }
                                    )
                            },
                            sEnabled = calendarOpened
                        }
                        .AsFlexGroup()
                        .AsRectMask()
                }
            }
            .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
            .Bind(ref _bar);

        return new Layout {
                FlexItem = {
                    Size = new() { y = 10.pt }
                },

                Children = {
                    content
                }
            }
            .AsFlexGroup()
            .Use();
    }

    #endregion
}
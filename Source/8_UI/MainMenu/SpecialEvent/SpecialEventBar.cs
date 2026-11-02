using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
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
                Children = {
                    new Image()
                        .AsBlurBackground(pixelsPerUnit: 10f)
                        .On(bgAlpha, (x, y) => x.Color = x.Color.ColorWithAlpha(y))
                        .AsFlexItem(position: 0.pt),

                    // Bar
                    new Layout {
                        Children = {
                            new BsTextButton {
                                    Text = "←",
                                    ShowUnderline = false,
                                    Skew = 0f,
                                    OnClick = () => OnBackClick?.Invoke()
                                }
                                .WithNativeComponent(out CanvasGroup backButtonGroup)
                                .On(itemsAlpha, (_, y) => backButtonGroup.alpha = y, lazy: true)
                                .AsFlexItem(
                                    size: new() { y = 90.pct, x = 8f },
                                    position: new() { left = 0.pt },
                                    alignSelf: Align.Center
                                ),

                            new BsTextButton {
                                    Text = " Play ",
                                    OnClick = () => OnPlayClick?.Invoke()
                                }
                                .WithNativeComponent(out CanvasGroup playButtonGroup)
                                .On(itemsAlpha, (_, y) => playButtonGroup.alpha = y, lazy: true)
                                .AsFlexItem(),

                            // Calendar button
                            new Layout {
                                Children = {
                                    new BsTextButton {
                                            Text = "💿",
                                            ShowUnderline = false,
                                            Skew = 0f,
                                            sEnabled = calendarOpened,

                                            OnClick = () => {
                                                ReeModalSystem.OpenModal<EventDetailsDialog>(ContentTransform, _event.Value.eventDescription);
                                            }
                                        }
                                        .AsFlexItem(
                                            position: new() { left = 0.pt },
                                            modifier: out var playlistButtonModifier
                                        )
                                        .On(
                                            calendarButtonWidth,
                                            (_, y) => playlistButtonModifier.Size = new() { x = y.pt, y = 100.pct }
                                        )
                                }
                            }.AsFlexGroup().AsFlexItem(
                                size: new() { x = initialCalendarButtonWidth.pt, y = 90.pct },
                                position: new() { left = 0.pt },
                                alignSelf: Align.Center
                            ),

                            new Layout {
                                Children = {
                                    new BsTextButton {
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
                                        .AsFlexItem(
                                            position: new() { right = 0.pt },
                                            modifier: out var buttonModifier
                                        )
                                        .On(
                                            calendarButtonWidth,
                                            (_, y) => buttonModifier.Size = new() { x = y.pt, y = 100.pct }
                                        )
                                }
                            }.AsFlexGroup().AsFlexItem(
                                size: new() { x = initialCalendarButtonWidth.pt, y = 90.pct },
                                position: new() { right = 0.pt },
                                alignSelf: Align.Center
                            )
                        }
                    }.AsFlexGroup(
                        justifyContent: Justify.Center,
                        alignItems: Align.Stretch,
                        gap: 2.pt
                    ).AsFlexItem(
                        size: new() { x = 100.pct, y = 8.pt }
                    ),

                    // Calendar
                    new Layout {
                            WithinLayoutIfDisabled = true,
                            Children = {
                                new EventCalendar {
                                        OnDayChanged = x => OnDayChanged?.Invoke(x),
                                    }
                                    .AsFlexItem(
                                        position: new() { left = 0.pt, top = 0.pt, right = 0.pt },
                                        positionType: PositionType.Absolute,
                                        margin: new() { top = 1.pt, bottom = 2.pt }
                                    )
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
                        .AsFlexItem(modifier: out var calendarModifier)
                        .AsRectMask()
                        .On(calendarHeight, (_, y) => calendarModifier.Size = new() { y = y.pt }, lazy: true)
                }
            }
            .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
            .AsFlexGroup(direction: FlexDirection.Column, padding: 1f)
            .AsFlexItem(minSize: 100.pct, size: new() { x = 100.pct }, position: new() { bottom = 0.pt })
            .Bind(ref _bar);

        return new Layout {
                Children = {
                    content
                }
            }
            .AsFlexGroup()
            .AsFlexItem(size: new() { y = 10.pt })
            .Use();
    }

    #endregion
}
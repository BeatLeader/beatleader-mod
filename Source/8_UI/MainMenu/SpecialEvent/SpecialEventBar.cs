using System;
using BeatLeader.Models;
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

    public void SetData(PlatformEventStatus status) {
        _event.Value = status;
    }

    #endregion

    #region Construct

    private ObservableValue<PlatformEventStatus> _event = null!;
    private ReactiveComponent _bar = null!;
    private ReactiveComponent _calendar = null!;

    protected override GameObject Construct() {
        _event = Remember<PlatformEventStatus>(null!);

        var initialCalendarButtonWidth = 8f;

        var calendarOpened = Remember(false);
        var calendarHeight = RememberAnimated(0f, 200.ms(), AnimationCurve.Exponential);
        var calendarButtonWidth = RememberAnimated(initialCalendarButtonWidth, 150.ms(), AnimationCurve.EaseInOut);

        var itemsAlpha = RememberAnimated(1f, 150.ms(), AnimationCurve.EaseInOut);
        var bgAlpha = RememberAnimated(0f, 150.ms(), AnimationCurve.EaseInOut);

        var content = new Background {
                Children = {
                    new Image()
                        .AsBlurBackground(pixelsPerUnit: 10f)
                        .Animate(bgAlpha, (x, y) => x.Color = x.Color.ColorWithAlpha(y), applyImmediately: true)
                        .AsFlexItem(position: 0.pt()),

                    // Bar
                    new Layout {
                        Children = {
                            new Label {
                                    Alignment = TextAlignmentOptions.Capline
                                }
                                .Animate(
                                    _event,
                                    (x, y) => {
                                        if (y.today != null) x.Text = FormatUtils.GetRemainingTime(y.today.ExpiresIn());
                                    }
                                )
                                .Animate(itemsAlpha, (x, y) => x.Color = Color.white * 0.7f * y, applyImmediately: true)
                                .AsFlexItem(
                                    position: new() { left = 0.pt() },
                                    alignSelf: Align.Center
                                ),

                            new BsButton {
                                    Text = " Play ",
                                    OnClick = () => OnPlayClick?.Invoke()
                                }
                                .WithNativeComponent(out CanvasGroup playButtonGroup)
                                .Animate(itemsAlpha, (_, y) => playButtonGroup.alpha = y)
                                .AsFlexItem(),

                            // Calendar button
                            new Layout {
                                Children = {
                                    new BsButton {
                                            Text = "📅",
                                            ShowUnderline = false,
                                            Skew = 0f,

                                            OnClick = () => {
                                                calendarOpened.Value = !calendarOpened;
                                                calendarButtonWidth.Value = calendarOpened ? _bar.ContentTransform.rect.width - 2f : initialCalendarButtonWidth;
                                                calendarHeight.Value = calendarOpened ? _calendar.ContentTransform.rect.height + 3 : 0f;

                                                itemsAlpha.Value = calendarOpened ? 0f : 1f;
                                                bgAlpha.Value = calendarOpened ? 1f : 0f;
                                            }
                                        }
                                        .AsFlexItem(
                                            position: new() { right = 0.pt() },
                                            modifier: out var buttonModifier
                                        )
                                        .Animate(
                                            calendarButtonWidth,
                                            (_, y) => buttonModifier.Size = new() { x = y.pt(), y = 100.pct() },
                                            applyImmediately: true
                                        )
                                }
                            }.AsFlexGroup().AsFlexItem(
                                size: new() { x = initialCalendarButtonWidth.pt(), y = 90.pct() },
                                position: new() { right = 0.pt() },
                                alignSelf: Align.Center
                            )
                        }
                    }.AsFlexGroup(
                        justifyContent: Justify.Center,
                        alignItems: Align.Stretch,
                        gap: 2.pt()
                    ).AsFlexItem(
                        size: new() { x = 100.pct(), y = 8.pt() }
                    ),

                    // Calendar
                    new Layout {
                            Children = {
                                new EventCalendar()
                                    .AsFlexItem(
                                        position: new() { left = 0.pt(), top = 0.pt(), right = 0.pt() },
                                        margin: new() { top = 1.pt(), bottom = 2.pt() }
                                    )
                                    .Animate(_event, (x, y) => x.SetData(y))
                                    .Bind(ref _calendar)
                            }
                        }
                        .AsFlexGroup()
                        .AsFlexItem(modifier: out var calendarModifier)
                        .WithNativeComponent(out RectMask2D _)
                        .Animate(calendarHeight, (_, y) => calendarModifier.Size = new() { y = y.pt() })
                }
            }
            .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
            .AsFlexGroup(direction: FlexDirection.Column, padding: 1f)
            .AsFlexItem(minSize: 100.pct(), size: new() { x = 100.pct() }, position: new() { bottom = 0.pt() })
            .Bind(ref _bar);

        return new Layout {
                Children = {
                    content
                }
            }
            .AsFlexGroup()
            .AsFlexItem(size: new() { x = 50.pt(), y = 10.pt() })
            .Use();
    }

    #endregion
}
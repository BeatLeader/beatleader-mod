using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.WebRequests;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.MainMenu;

internal class NewsViewPanel : ReactiveComponent {
    #region Public API

    public void SetEvents(RequestState state, IReadOnlyList<PlatformEvent>? events) {
        _eventNewsPanel.SetData(state, events);
    }

    public void SetSpecialEvent(PlatformEventStatus? status) {
        _specialEvent.Value = status;
    }

    #endregion

    #region Construct

    private EventNewsPanel _eventNewsPanel = null!;
    private ObservableValue<PlatformEventStatus?> _specialEvent = null!;

    protected override GameObject Construct() {
        var selectorHeight = RememberAnimated(0f, 150.ms(), AnimationCurve.EaseInOut);

        _specialEvent = Remember<PlatformEventStatus?>(null);
        _specialEvent.ValueChangedEvent += x => {
            selectorHeight.Value = x != null ? 5f : 0f;
        };

        return new Layout {
                Children = {
                    new TextNewsPanel(),

                    new Layout {
                            Children = {
                                // Selector
                                new Background {
                                        Children = {
                                            new TextSegmentedControl<string> {
                                                    Items = {
                                                        ["default"] = "<i>Main",
                                                        ["special"] = "<i>Special"
                                                    }
                                                }
                                                .AsFlexItem(flexGrow: 1f)
                                                .Export(out var control)
                                        }
                                    }
                                    .WithNativeComponent(out RectMask2D _)
                                    .AsBackground(color: Color.black.ColorWithAlpha(0.5f))
                                    .AsFlexGroup(alignItems: Align.Stretch)
                                    .AsFlexItem(margin: new() { top = 1f }, modifier: out var modifier)
                                    .Animate(
                                        selectorHeight,
                                        onEffect: (_, y) => modifier.Size = new() { y = y.pt() },
                                        applyImmediately: true
                                    ),

                                new KeyedContainer<string> {
                                        Control = control,
                                        Items = {
                                            ["default"] = new Layout {
                                                    Children = {
                                                        new EventNewsPanel().Bind(ref _eventNewsPanel),
                                                        new MapNewsPanel(),
                                                    }
                                                }
                                                .AsFlexGroup(direction: FlexDirection.Column, gap: 1f)
                                                .AsFlexItem(size: 100.pct()),

                                            ["special"] = new SpecialEventPanel()
                                                .Animate(
                                                    _specialEvent,
                                                    (x, y) => {
                                                        if (y != null) x.SetData(y);
                                                    }
                                                )
                                        }
                                    }
                                    .AsFlexGroup(direction: FlexDirection.Column)
                                    .AsFlexItem(flex: 1f, flexShrink: 1f)
                            }
                        }
                        .AsFlexGroup(direction: FlexDirection.Column)
                        .AsFlexItem(
                            flex: 1f,
                            size: new() { y = 70f }
                        )
                }
            }
            .AsFlexGroup(
                direction: FlexDirection.Row,
                constrainHorizontal: false,
                constrainVertical: false,
                gap: 1f,
                padding: new() { left = 30f }
            )
            .AsFlexItem()
            .Use();
    }

    #endregion
}
using BeatLeader.Models;
using BeatLeader.UI;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.Results;

public class SpecialEventResultsPanel : ReactiveComponent {
    #region Public API

    public void SetEvent(PlatformEventStatus status) {
        _status.Value = status;
        _contentAlpha.TargetValue = 1f;
    }

    #endregion

    #region Construct

    private State<PlatformEventStatus?> _status = null!;
    private AnimatedState<float> _contentAlpha = null!;

    protected override GameObject Construct() {
        _contentAlpha = RememberAnimated(0f, 200.ms, AnimationCurve.EaseInOut);
        _status = Remember<PlatformEventStatus?>(null);

        return new Layout {
            FlexController = {
                Gap = 1.pt
            },

            FlexItem = {
                Size = new() { x = 60.pt, y = 10.pt }
            },

            Children = {
                new WebImage {
                    FlexItem = {
                        Size = 10f
                    },

                    Sprite = BundleLoader.UnknownIcon,
                    Material = BundleLoader.RoundTextureMaterial,
                    Skew = UIStyle.Skew,
                    sSrc = _status.Map(x => x?.eventDescription.image).InLazy()
                }.WithAlpha(_contentAlpha),

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
                            sText = _status.Map(x => $"{x?.eventDescription.name} day #{x?.today?.day}").InLazy()
                        },

                        new Label {
                            FlexItem = {
                                Margin = new() { left = -0.6f }
                            },

                            FontSize = 3.5f,
                            FontStyle = FontStyles.Italic,
                            Overflow = TextOverflowModes.Ellipsis,
                            Alignment = TextAlignmentOptions.TopLeft,
                            RichText = true,
                            sText = _status.Map(
                                x => x?.today?.points is { } points
                                    ? $"{points.rank} on the leaderboard - {points.points} points!"
                                    : "No score yet"
                            ).InLazy()
                        }
                    }
                }.WithAlpha(_contentAlpha),

                new Spinner {
                    FlexItem = {
                        Position = 0.pt,
                        PositionType = PositionType.Absolute
                    },

                    Image = {
                        RaycastTarget = false
                    }
                }.WithAlpha(_contentAlpha, invert: true)
            }
        }.Use();
    }

    #endregion
}
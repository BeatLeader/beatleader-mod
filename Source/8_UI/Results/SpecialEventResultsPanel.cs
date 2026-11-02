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
                Children = {
                    new WebImage {
                            Sprite = BundleLoader.UnknownIcon,
                            Material = BundleLoader.RoundTextureMaterial,
                            Skew = UIStyle.Skew,
                            sSrc = _status.Map(x => x?.eventDescription.image).InLazy()
                        }
                        .AsFlexItem(size: 10f)
                        .WithAlpha(_contentAlpha),

                    new Layout {
                            Children = {
                                new Label {
                                    FontSize = 4f,
                                    FontStyle = FontStyles.Italic,
                                    Overflow = TextOverflowModes.Ellipsis,
                                    Alignment = TextAlignmentOptions.CaplineLeft,
                                    sText = _status.Map(x => $"{x?.eventDescription.name} day #{x?.today?.day}").InLazy()
                                }.AsFlexItem(),

                                new Label {
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
                                }.AsFlexItem(margin: new() { left = -0.6f })
                            }
                        }
                        .WithAlpha(_contentAlpha)
                        .AsFlexGroup(direction: FlexDirection.Column)
                        .AsFlexItem(margin: new() { left = 1 }, flexGrow: 1, flexShrink: 1),

                    new Spinner {
                            Image = {
                                RaycastTarget = false
                            }
                        }
                        .WithAlpha(_contentAlpha, invert: true)
                        .AsFlexItem(position: 0.pt)
                }
            }
            .AsFlexGroup(gap: 1.pt)
            .AsFlexItem(size: new() { x = 60.pt, y = 10.pt })
            .Use();
    }

    #endregion
}
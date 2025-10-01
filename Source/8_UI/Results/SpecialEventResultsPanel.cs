using BeatLeader.Models;
using BeatLeader.UI;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.Results;

public class SpecialEventResultsPanel : ReactiveComponent {
    #region Public API

    public void SetEvent(PlatformEventStatus status) {
        _status.Value = status;
        _contentAlpha.Value = 1f;
    }

    public void SetLoading() {
        _contentAlpha.Value = 0f;
    }

    #endregion

    #region Construct

    private ObservableValue<PlatformEventStatus?> _status = null!;
    private AnimatedValue<float> _contentAlpha = null!;

    protected override GameObject Construct() {
        _contentAlpha = RememberAnimated(0f, 200.ms(), AnimationCurve.EaseInOut);
        _status = Remember<PlatformEventStatus?>(null);

        return new Layout {
                Children = {
                    new Image {
                            Sprite = BundleLoader.UnknownIcon,
                            Material = BundleLoader.RoundTextureMaterial,
                            Skew = UIStyle.Skew
                        }
                        .AsFlexItem(size: new() { x = 10, y = 10 })
                        .WithAlpha(_contentAlpha)
                        .Animate(
                            _status,
                            (img, item) => {
                                if (item != null) {
                                    img.WithWebSource(item.eventDescription.image);
                                }
                            }
                        ),

                    new Layout {
                            Children = {
                                new Label {
                                        FontSize = 4f,
                                        FontStyle = FontStyles.Italic,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        Alignment = TextAlignmentOptions.CaplineLeft
                                    }
                                    .AsFlexItem()
                                    .Animate(
                                        _status,
                                        (lbl, item) => {
                                            if (item != null) {
                                                lbl.Text = $"{item.eventDescription.name} day #{item.today?.day}";
                                            }
                                        }
                                    ),

                                new Label {
                                        FontSize = 3.5f,
                                        FontStyle = FontStyles.Italic,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        Alignment = TextAlignmentOptions.TopLeft,
                                        RichText = true,
                                    }
                                    .AsFlexItem(margin: new() { left = -0.6f })
                                    .Animate(
                                        _status,
                                        (lbl, item) => {
                                            if (item?.today?.points is { } points) {
                                                lbl.Text = $"{points.rank} on the leaderboard - {points.points} points!";
                                            } else {
                                                lbl.Text = "No score yet";
                                            }
                                        }
                                    )
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
                        .AsFlexItem(position: 0.pt())
                }
            }
            .AsFlexGroup(gap: 1.pt())
            .AsFlexItem(size: new() { x = 60.pt(), y = 10.pt() })
            .Use();
    }

    #endregion
}
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

internal class DanceEventResultsPanel : ReactiveComponent {
    #region Public API

    public void SetEvent(PlatformEvent evt) {
        _event.Value = evt;
        _standing.Value = null;
    }

    public void SetLoading() {
        _contentAlpha.Value = 0f;
    }

    public void SetResult(string text, string? standing = null) {
        _result.Value = text;
        _standing.Value = standing;
        _contentAlpha.Value = 1f;
    }

    #endregion

    #region Construct

    private ObservableValue<PlatformEvent?> _event = null!;
    private ObservableValue<string> _result = null!;
    private ObservableValue<string?> _standing = null!;
    private AnimatedValue<float> _contentAlpha = null!;

    protected override GameObject Construct() {
        _contentAlpha = RememberAnimated(0f, 200.ms(), AnimationCurve.EaseInOut);
        _event = Remember<PlatformEvent?>(null);
        _result = Remember("");
        _standing = Remember<string?>(null);

        return new Layout {
                Children = {
                    new Image {
                            Sprite = BundleLoader.UnknownIcon,
                            Material = BundleLoader.RoundTextureMaterial,
                            Skew = UIStyle.Skew
                        }
                        .AsFlexItem(size: new() { x = 10, y = 10 })
                        .Animate(
                            _event,
                            (img, evt) => {
                                if (evt != null) img.WithWebSource(evt.image);
                            }
                        ),

                    new Layout {
                            Children = {
                                new Label {
                                        FontSize = 4f,
                                        FontStyle = FontStyles.Italic,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        Alignment = TextAlignmentOptions.CaplineLeft,
                                        RichText = true
                                    }
                                    .AsFlexItem()
                                    .Animate(_event, (lbl, _) => lbl.Text = Title())
                                    .Animate(_standing, (lbl, _) => lbl.Text = Title()),

                                new Label {
                                        FontSize = 3.5f,
                                        FontStyle = FontStyles.Italic,
                                        Overflow = TextOverflowModes.Ellipsis,
                                        Alignment = TextAlignmentOptions.TopLeft,
                                        RichText = true
                                    }
                                    .AsFlexItem(margin: new() { left = -0.6f })
                                    .WithAlpha(_contentAlpha)
                                    .Animate(_result, (lbl, text) => lbl.Text = text)
                            }
                        }
                        .AsFlexGroup(direction: FlexDirection.Column)
                        .AsFlexItem(margin: new() { left = 1 }, flexGrow: 1, flexShrink: 1),

                    new Spinner {
                            Image = {
                                RaycastTarget = false
                            }
                        }
                        .WithAlpha(_contentAlpha, invert: true)
                        .AsFlexItem(size: new() { x = 5, y = 5 }, alignSelf: Align.Center)
                }
            }
            .AsFlexGroup(gap: 1.pt())
            .AsFlexItem(size: new() { x = 70.pt(), y = 10.pt() })
            .Use();
    }

    private string Title() {
        var name = _event.Value?.name ?? "";
        return _standing.Value is { } standing ? $"{name}  <color=#AAAAAA>{standing}</color>" : name;
    }

    #endregion
}

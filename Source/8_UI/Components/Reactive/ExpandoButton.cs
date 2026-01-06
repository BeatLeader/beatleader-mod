using System;
using JetBrains.Annotations;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Reactive.Components;

/// <summary>
/// A button that is usually used with <see cref="ExpandoGroup"/>.
/// When collapsed shows an icon, when expanded shows a text.
/// </summary>
[PublicAPI]
public class ExpandoButton : ReactiveComponent {
    #region Props

    public Sprite Icon {
        get => _icon.Sprite!;
        set => _icon.Sprite = value;
    }

    public string Text {
        get => _text.Text;
        set => _text.Text = value;
    }

    public FontStyles FontStyle {
        get => _text.FontStyle;
        set => _text.FontStyle = value;
    }

    public Color Color {
        get;
        set {
            field = value;
            _icon.Color = value.ColorWithAlpha(-_crossfadeFactor.CurrentValue);
            _text.Color = value.ColorWithAlpha(_crossfadeFactor.CurrentValue);
        }
    }

    public AnimationDuration CrossfadeDuration {
        get => _crossfadeFactor.Duration;
        set => _crossfadeFactor.Duration = value;
    }

    public AnimationCurve CrossfadeCurve {
        get => _crossfadeFactor.Curve;
        set => _crossfadeFactor.Curve = value;
    }
    
    public Action? OnClick { get; set; }

    public bool Clickable {
        get => _clickable.Value;
        set => _clickable.Value = value;
    }

    #endregion

    #region Construct

    private ObservableValue<bool> _clickable = null!;
    private AnimatedValue<float> _crossfadeFactor = null!;
    private Label _text = null!;
    private Image _icon = null!;

    protected override GameObject Construct() {
        _clickable = Remember(true);
        _crossfadeFactor = RememberAnimated(0f, 200.ms(), AnimationCurve.EaseInOutExpo);

        return new BsButtonLayout {
                Children = {
                    new Image()
                        .Animate(_crossfadeFactor, (x, y) => x.Color = Color.ColorWithAlpha(-y))
                        .AsFlexItem(position: 0f)
                        .Bind(ref _icon),

                    new Label()
                        .With(x => {
                                x.LeafLayoutUpdatedEvent += y => {
                                    var measures = y.Measure(0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined);
                                    var buttonWidth = ContentTransform.rect.width;
                                    var padding = 1f; // 2x 0.5f

                                    _crossfadeFactor.Value = measures.x >= buttonWidth - padding ? 1f : 0f;
                                };
                            }
                        )
                        .Animate(_crossfadeFactor, (x, y) => x.Color = Color.ColorWithAlpha(y))
                        .AsFlexItem(position: 0f)
                        .Bind(ref _text)
                }
            }
            .Animate(_clickable, (x, y) => x.Interactable = y)
            .AsFlexGroup(padding: 0.5f)
            .Use();
    }

    #endregion
}
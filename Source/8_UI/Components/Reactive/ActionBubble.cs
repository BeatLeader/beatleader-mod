using System.Linq;
using JetBrains.Annotations;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Reactive.Components;

[PublicAPI]
internal class ActionBubble : ReactiveComponent {
    #region Spawn

    public const float DefaultDuration = 1.6f;

    private const float RiseDistance = 6f;

    private static readonly Color OkColor = new(0.35f, 0.9f, 0.5f);
    private static readonly Color InfoColor = new(0.35f, 0.65f, 1f);
    private static readonly Color ErrorColor = new(1f, 0.4f, 0.4f);

    private static readonly ReactivePool<ActionBubble> Pool = new();

    public static void Ok(RectTransform target, string text, float duration = DefaultDuration) {
        Spawn(target, BundleLoader.Sprites.checkIcon, text, OkColor, duration);
    }

    public static void Info(RectTransform target, string text, float duration = DefaultDuration) {
        Spawn(target, BundleLoader.NotificationIcon, text, InfoColor, duration);
    }

    public static void Error(RectTransform target, string text, float duration = DefaultDuration) {
        Spawn(target, BundleLoader.WarningIcon, text, ErrorColor, duration);
    }

    public static void Spawn(RectTransform target, Sprite icon, string text, Color color, float duration = DefaultDuration) {
        if (target.GetComponentInParent<Canvas>() is not { } canvas) {
            return;
        }

        Pool.Spawn().Show(canvas.rootCanvas.transform, target, icon, text, color, duration);
    }

    private void Show(Transform parent, RectTransform target, Sprite icon, string text, Color color, float duration) {
        Use(parent);

        ContentTransform.SetAsLastSibling();
        ContentTransform.position = target.TransformPoint(target.rect.center);
        ContentTransform.rotation = target.rotation;

        var origin = ContentTransform.localPosition;

        _icon.Value = icon;
        _text.Value = text;
        _color.Value = color;

        _position.Timing = duration.sec;
        _position.SetValueImmediate(origin);
        _position.TargetValue = origin + Vector3.up * RiseDistance;

        _scale.SetValueImmediate(Vector3.one * 0.6f);
        _scale.TargetValue = Vector3.one;

        _alpha.Timing = duration.sec;
        _alpha.SetValueImmediate(1f);
        _alpha.TargetValue = 0f;
    }

    private void HandleFinished(float alpha) {
        if (alpha > 0f || !Pool.SpawnedComponents.Contains(this)) {
            return;
        }

        Pool.Despawn(this);
    }

    #endregion

    #region Construct

    private State<Sprite?> _icon = null!;
    private State<string> _text = null!;
    private State<Color> _color = null!;
    private AnimatedState<Vector3> _position = null!;
    private AnimatedState<Vector3> _scale = null!;
    private AnimatedState<float> _alpha = null!;

    protected override GameObject Construct() {
        _icon = Remember<Sprite?>(null);
        _text = Remember("");
        _color = Remember(Color.white);
        _position = RememberAnimated(Vector3.zero, DefaultDuration.sec, AnimationCurve.EaseOut);
        _scale = RememberAnimated(Vector3.one, 300.ms, AnimationCurve.EaseOutBack);
        _alpha = RememberAnimated(1f, DefaultDuration.sec, AnimationCurve.EaseIn, HandleFinished);

        return new Background {
            FlexController = {
                AlignItems = Align.Center,
                Padding = new() { left = 1.2f, right = 1.6f, top = 0.6f, bottom = 0.6f },
                Gap = 0.8f,
                ConstrainHorizontal = false,
                ConstrainVertical = false
            },

            ContentTransform = {
                slocalPosition = _position,
                slocalScale = _scale
            },

            RaycastTarget = false,

            Children = {
                new Image {
                    FlexItem = {
                        Size = 3f
                    },

                    PreserveAspect = true,
                    sSprite = _icon,
                    sColor = _color
                },

                new Label {
                    FontSize = 3f,
                    FontStyle = FontStyles.Bold,
                    Alignment = TextAlignmentOptions.Capline,
                    EnableWrapping = false,
                    sText = _text
                }
            }
        }.AsBlurBackground().WithAlpha(_alpha).Use();
    }

    #endregion
}

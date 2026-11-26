using System;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;

namespace BeatLeader.UI.Hub {
    internal class ProfileTag : ReactiveComponent {
        #region Props

        public string Text {
            get => _text.Value;
            set => _text.Value = value;
        }

        public Color Color {
            get => _color.Value;
            set => _color.Value = value;
        }

        public float FontSize {
            get => _fontSize.Value;
            set => _fontSize.Value = value;
        }

        public float Appear {
            get => _appear.Value;
            set => _appear.Value = value;
        }

        public bool Interactable {
            get => _interactable.Value;
            set => _interactable.Value = value;
        }

        public Action? OnClick { get; set; }

        #endregion

        #region Construct

        private const float HoverGrowth = 1.5f;
        private const float MaxHoverScale = 1.1f;

        private State<string> _text = null!;
        private State<Color> _color = null!;
        private State<float> _fontSize = null!;
        private State<float> _appear = null!;
        private State<bool> _interactable = null!;

        protected override GameObject Construct() {
            _text = Remember("");
            _color = Remember(Color.white);
            _fontSize = Remember(2.6f);
            _appear = Remember(1f);
            _interactable = Remember(true);

            var hover = RememberAnimated(1f, 300.ms, AnimationCurve.EaseOutBack);
            var alpha = _appear.Map(static x => Mathf.Clamp01(x * 2f));
            var scale = RememberDerived(_appear, hover, static (x, y) => Vector3.one * Mathf.LerpUnclamped(0.4f, 1f, AnimationCurve.EaseOutBack.Evaluate(x)) * y);

            return new Background {
                FlexController = {
                    Padding = new() { left = 1.2f, right = 1.2f, top = 0.2f, bottom = 0.2f },
                    JustifyContent = Justify.Center,
                    AlignItems = Align.Center
                },

                FlexItem = {
                    sPosition = _appear.Map(static x => new YogaFrame { top = -Mathf.Sin(Mathf.Clamp01(x) * Mathf.PI) * 1.2f })
                },

                ContentTransform = {
                    slocalScale = scale
                },

                PixelsPerUnit = 10f,
                Sprite = BundleLoader.Sprites.background,
                Material = global::Reactive.BeatSaber.GameResources.UINoGlowMaterial,
                sColor = _color.Map(static x => Color.Lerp(x, Color.black, 0.55f)),

                Children = {
                    new Label {
                        sFontSize = _fontSize,
                        FontStyle = FontStyles.Bold,
                        Alignment = TextAlignmentOptions.Capline,
                        EnableWrapping = false,
                        sColor = _color.Map(static x => Color.Lerp(x, Color.white, 0.6f)),
                        sText = _text
                    }
                }
            }.WithAlpha(alpha).WithPointerEvents(
                onEnter: _ => hover.TargetValue = Interactable ? Mathf.Min(1f + HoverGrowth / Mathf.Max(ContentTransform.rect.width, HoverGrowth), MaxHoverScale) : 1f,
                onLeave: _ => hover.TargetValue = 1f,
                onDown: _ => {
                    if (OnClick == null || !Interactable) return;
                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                    OnClick();
                }
            ).Use();
        }

        #endregion
    }
}

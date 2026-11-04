using System;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class CheckCircle : ReactiveComponent {
        #region Public API

        public bool Active {
            get => _active.Value;
            set {
                if (value == _active.Value) {
                    return;
                }

                _active.Value = value;
                _hoverProgress.SetValueImmediate(0f);
                OnStateChanged?.Invoke(value);
            }
        }

        public Action<bool>? OnStateChanged { get; set; }

        #endregion

        #region Construct

        private State<bool> _active = null!;
        private AnimatedState<float> _hoverProgress = null!;

        protected override GameObject Construct() {
            _active = Remember(false);
            _hoverProgress = RememberAnimated(0f, 10.fact);

            var color = UIStyle.GlowingButtonColorSet.Color;
            var activeColor = UIStyle.GlowingButtonColorSet.GetColor(GraphicState.Active);

            return new Background {
                    Sprite = BundleLoader.Sprites.inactiveCheckIcon,
                    Material = BundleLoader.UIAdditiveGlowMaterial,
                    PreserveAspect = true,
                    sColor = RememberDerived(
                        _active, _hoverProgress,
                        (active, progress) => active ? Color.clear : Color.Lerp(color, Color.clear, progress)
                    ),

                    Children = {
                        new Image {
                            Sprite = BundleLoader.Sprites.checkIcon,
                            Material = BundleLoader.UIAdditiveGlowMaterial,
                            PreserveAspect = true,
                            RaycastTarget = false,
                            sColor = RememberDerived(
                                _active, _hoverProgress,
                                (active, progress) => active ? activeColor : Color.Lerp(Color.clear, activeColor, progress)
                            )
                        }.WithRectExpand()
                    }
                }
                .WithPointerEvents(
                    onEnter: _ => _hoverProgress.TargetValue = 1f,
                    onLeave: _ => _hoverProgress.TargetValue = 0f,
                    onDown: _ => Active = !Active
                )
                .WithHoverScale(1f, 1.2f)
                .Use();
        }

        #endregion
    }
}
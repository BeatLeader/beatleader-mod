using System;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class CheckCircle : ReactiveComponent {
        #region Public API

        public bool Active => _active.Value;
        public Action<bool>? OnStateChanged { get; set; }

        public void SetActive(bool active, bool notifyListeners = true, bool force = false) {
            if (!force && active == Active) {
                return;
            }

            _active.Value = active;
            _hoverProgress.SetValueImmediate(0f);

            if (notifyListeners) {
                OnStateChanged?.Invoke(active);
            }
        }

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
                    onDown: _ => SetActive(!Active)
                )
                .WithHoverScale(1f, 1.2f)
                .Use();
        }

        #endregion
    }
}
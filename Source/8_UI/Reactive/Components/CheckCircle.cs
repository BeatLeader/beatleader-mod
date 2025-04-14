using System;
using Reactive;
using Reactive.Components;
using Reactive.Components.Basic;
using UnityEngine;
using Image = Reactive.BeatSaber.Components.Image;

namespace BeatLeader.UI.Reactive.Components {
    internal class CheckCircle : ReactiveComponent {
        public bool Active { get; private set; }
        public Action<bool>? OnStateChanged { get; set; }

        private ButtonBase _button = null!;
        private Image _inactiveImage = null!;
        private Image _activeImage = null!;

        public void SetActive(bool active, bool notifyListeners = true, bool force = false) {
            _button.Click(active, notifyListeners, force);
        }

        private void RefreshColors(float t) {
            var color = UIStyle.ButtonColorSet.Color;
            var activeColor = UIStyle.ButtonColorSet.ActiveColor;

            if (!Active) {
                _inactiveImage.Color = Color.Lerp(color, Color.clear, t);
                _activeImage.Color = Color.Lerp(Color.clear, activeColor, t);
            } else {
                _inactiveImage.Color = Color.clear;
                _activeImage.Color = activeColor;
            }
        }

        protected override GameObject Construct() {
            var progress = RememberAnimated(0f, 10.fact());

            progress.ValueChangedEvent += RefreshColors;

            return new Clickable {
                OnStateChanged = x => {
                    Active = x;
                    RefreshColors(0f);
                    OnStateChanged?.Invoke(x);
                },
                Latching = true,
                Children = {
                    new Image {
                        Sprite = BundleLoader.Sprites.inactiveCheckIcon,
                        Material = BundleLoader.UIAdditiveGlowMaterial,
                        PreserveAspect = true
                    }.WithRectExpand().Bind(ref _inactiveImage),

                    new Image {
                        Sprite = BundleLoader.Sprites.checkIcon,
                        Material = BundleLoader.UIAdditiveGlowMaterial,
                        PreserveAspect = true
                    }.WithRectExpand().Bind(ref _activeImage)
                }
            }.WithListener(
                x => x.IsHovered,
                x => progress.Value = x ? 1f : 0f
            ).WithScaleAnimation(1f, 1.2f).Bind(ref _button).Use();
        }
    }
}
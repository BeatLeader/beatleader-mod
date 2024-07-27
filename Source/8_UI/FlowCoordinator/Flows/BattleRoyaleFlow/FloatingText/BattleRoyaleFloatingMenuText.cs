using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleFloatingMenuText : ReactiveComponent {
        #region Animation

        private readonly ValueAnimator _animator = new() { LerpCoefficient = 15f };

        public void Present() {
            _animator.Push();
        }

        public void Hide() {
            _animator.Pull();
        }

        protected override void OnUpdate() {
            _animator.Update();
            _canvasGroup.alpha = _animator.Progress;
        }

        #endregion

        #region Construct

        private CanvasGroup _canvasGroup = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Label {
                        Text = "Did you see it?! Monke stole all players and mystically disappeared!\n" +
                            "But don't be upset, add some by yourself!",
                        Color = UIStyle.SecondaryTextColor,
                        FontSize = 6f
                    }
                }
            }.WithNativeComponent(out _canvasGroup).Use();
        }

        protected override void OnInitialize() {
            ReactiveUtils.AddCanvas(this);
            Content.AddComponent<CurvedCanvasSettings>();
            ContentTransform.localScale = Vector3.one * 0.02f;
            var floating = Content.AddComponent<FloatingObject>();
            floating.amplitude = 0.04f;
            floating.speed = 0.8f;
            floating.rotationAmplitude = 5f;
            floating.rotationSpeed = 1f;
        }

        #endregion
    }
}
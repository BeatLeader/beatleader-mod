using BeatLeader.UI.Reactive.Components;
using HMUI;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleFloatingMenuText : ReactiveComponent {
        #region Animation

        public bool IsPresent {
            set => _alpha.TargetValue = value ? 1f : 0f;
        }

        private AnimatedState<float> _alpha = null!;

        #endregion

        #region Construct

        protected override GameObject Construct() {
            _alpha = RememberAnimated(0f, 15.fact);

            return new Layout {
                Children = {
                    new Label {
                        Text = "Did you see it?! Monke stole all players and mystically disappeared!\n" +
                            "But don't be upset, add some by yourself!",
                        Color = BeatSaberStyle.SecondaryTextColor,
                        FontSize = 6f
                    }
                }
            }.WithAlpha(_alpha).Use();
        }

        protected override void OnInitialize() {
            BeatSaberUtils.AddCanvas(this);
            
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
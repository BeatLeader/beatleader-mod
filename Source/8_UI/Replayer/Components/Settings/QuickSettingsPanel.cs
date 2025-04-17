using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;
using AnimationCurve = Reactive.AnimationCurve;
using Image = Reactive.BeatSaber.Components.Image;

namespace BeatLeader.UI.Replayer {
    internal class QuickSettingsPanel : ReactiveComponent {
        private const float Height = 10f;

        private SpeedSlider _speedSlider = null!;
        private AnimatedValue<float> _contentAnim = null!;
        private YogaModifier _modifier = null!;

        public void Setup(IBeatmapTimeController timeController) {
            _speedSlider.Setup(timeController);
        }

        public void SetShown(bool shown, bool immediate) {
            var value = shown ? Height : 0f;
            if (immediate) {
                _contentAnim.SetValueImmediate(value);
            } else {
                _contentAnim.Value = value;
            }
        }

        protected override GameObject Construct() {
            _contentAnim = RememberAnimated(0f, 150f.ms(), AnimationCurve.EaseOut);

            return new Background {
                Children = {
                    new Layout {
                        ContentTransform = {
                            sizeDelta = new(0f, Height)
                        },
                        Children = {
                            new SpeedSlider().Bind(ref _speedSlider)
                        }
                    }.AsFlexItem(size: "100%").AsFlexGroup(
                        direction: FlexDirection.Column,
                        padding: new() { top = 1f, bottom = 1f, left = 2f, right = 2f }
                    )
                }
            }.AsFlexItem(modifier: out _modifier).AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart
            ).AsBackground(
                color: new(0.15f, 0.15f, 0.15f, 1f),
                sprite: BundleLoader.Sprites.backgroundRightBottom,
                pixelsPerUnit: 7f
            ).WithEffect(
                _contentAnim,
                (_, y) => {
                    var size = _modifier.Size;
                    size.y = y;
                    _modifier.Size = size;
                }
            ).WithNativeComponent(out Mask _).Use();
        }
    }
}
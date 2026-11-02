using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
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
        private AnimatedState<float> _contentAnim = null!;

        public void Setup(IBeatmapTimeController timeController) {
            _speedSlider.Setup(timeController);
        }

        public bool IsPresent {
            get => _contentAnim.TargetValue > 0f;
            set {
                var height = value ? Height : 0f;

                if (Content.activeInHierarchy) {
                    _contentAnim.TargetValue = height;
                } else {
                    _contentAnim.SetValueImmediate(height);
                }
            }
        }

        protected override GameObject Construct() {
            _contentAnim = RememberAnimated(0f, 150f.ms, AnimationCurve.EaseOut);

            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.FlexStart
                },

                FlexItem = {
                    sSize = _contentAnim.Map(x => new YogaVector { y = x }).InLazy()
                },

                Children = {
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            Padding = new() { top = 1f, bottom = 1f, left = 2f, right = 2f }
                        },

                        FlexItem = {
                            Size = "100%"
                        },

                        ContentTransform = {
                            sizeDelta = new(0f, Height)
                        },
                        Children = {
                            new SpeedSlider().Bind(ref _speedSlider)
                        }
                    }
                }
            }.AsBackground(
                color: new(0.15f, 0.15f, 0.15f, 1f),
                sprite: BundleLoader.Sprites.backgroundRightBottom,
                pixelsPerUnit: 7f
            ).WithNativeComponent(out Mask _).Use();
        }
    }
}
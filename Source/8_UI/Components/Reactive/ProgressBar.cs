using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ProgressBar : ReactiveComponent {
        #region Props

        public float TotalProgress {
            get => _totalProgress.Value;
            set => _totalProgress.Value = value;
        }

        public float Progress {
            get => _progress.Value;
            set => _progress.Value = value;
        }

        public Color Color {
            get => _bar.Color;
            set => _bar.Color = value;
        }

        #endregion

        #region Construct

        private State<float> _totalProgress = null!;
        private State<float> _progress = null!;
        private Image _bar = null!;

        protected override GameObject Construct() {
            _totalProgress = Remember(1f);
            _progress = Remember(0f);

            return new Background {
                FlexItem = {
                    Size = new() { y = 2f }
                },

                Sprite = BundleLoader.Sprites.background,
                PixelsPerUnit = 15f,
                Color = (Color.white * 0.2f).ColorWithAlpha(0.5f),

                Children = {
                    new Image {
                        ContentTransform = {
                            anchorMin = Vector2.zero,
                            sizeDelta = Vector2.zero,
                            sanchorMax = RememberDerived(
                                _progress, _totalProgress,
                                static (progress, total) => new Vector2(Mathf.Clamp01(progress / total), 1f)
                            )
                        },

                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 15f,
                        Color = BeatSaberStyle.BsToggleColors.KnobColors.GetColor(GraphicState.Active)
                    }.Bind(ref _bar)
                }
            }.Use();
        }

        #endregion
    }
}
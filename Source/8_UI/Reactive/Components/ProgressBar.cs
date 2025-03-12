using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ProgressBar : ReactiveComponent {
        #region Props

        public float TotalProgress {
            get => _totalProgress;
            set {
                _totalProgress = value;
                RefreshBarPos();
            }
        }

        public float Progress {
            get => _progress;
            set {
                _progress = value;
                RefreshBarPos();
            }
        }

        public Color Color {
            get => _progressBarImage.Color;
            set => _progressBarImage.Color = value;
        }

        private float _totalProgress = 1f;
        private float _progress;

        #endregion

        #region Construct

        private Image _progressBarImage = null!;
        private RectTransform _progressBar = null!;

        protected override GameObject Construct() {
            return new Image {
                Sprite = BundleLoader.Sprites.background,
                PixelsPerUnit = 15f,
                Color = (Color.white * 0.2f).ColorWithAlpha(0.5f),

                Children = {
                    new Image {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 15f,
                        Color = UIStyle.ControlButtonColorSet.ActiveColor
                    }.Bind(ref _progressBar).Bind(ref _progressBarImage)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { y = 2f });
            RefreshBarPos();
        }

        private void RefreshBarPos() {
            _progressBar.anchorMin = Vector2.zero;
            _progressBar.anchorMax = new Vector2(Mathf.Clamp01(Progress / TotalProgress), 1f);
            _progressBar.sizeDelta = Vector2.zero;
        }

        #endregion
    }
}
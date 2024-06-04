using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ImageButton : ColoredButton {
        #region UI Props

        public StateColorSet? GradientColors0 {
            get => _gradientColors0;
            set {
                _gradientColors0 = value;
                RefreshGradientState();
                UpdateColor();
                NotifyPropertyChanged();
            }
        }

        public StateColorSet? GradientColors1 {
            get => _gradientColors1;
            set {
                _gradientColors1 = value;
                RefreshGradientState();
                UpdateColor();
                NotifyPropertyChanged();
            }
        }

        private StateColorSet? _gradientColors0;
        private StateColorSet? _gradientColors1;

        private void RefreshGradientState() {
            Image.UseGradient = GradientColors0 != null || GradientColors1 != null;
        }

        #endregion

        #region UI Components

        public Image Image { get; private set; } = null!;

        #endregion

        #region Color

        protected override void ApplyColor(Color color) {
            Image.Color = color;
            Image.GradientColor0 = GetColor(_gradientColors0);
            Image.GradientColor1 = GetColor(_gradientColors1);
        }

        protected override void OnInteractableChange(bool interactable) {
            UpdateColor();
        }

        #endregion

        #region Setup

        private RectTransform _childrenContainerTransform = null!;

        protected override void Construct(RectTransform rect) {
            //background
            Image = new Image {
                Name = "Background"
            }.WithRectExpand();
            Image.Use(rect);
            //content
            _childrenContainerTransform = new GameObject("Content").AddComponent<RectTransform>();
            _childrenContainerTransform.SetParent(rect, false);
            _childrenContainerTransform.WithRectExpand();
            base.Construct(rect);
        }

        protected override void AppendReactiveChild(ReactiveComponentBase comp) {
            comp.Use(_childrenContainerTransform);
        }

        protected override void OnInitialize() {
            Image.Material = BundleLoader.UIAdditiveGlowMaterial;
        }

        #endregion
    }
}
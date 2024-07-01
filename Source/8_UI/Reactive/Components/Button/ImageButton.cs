using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class ImageButton : ColoredButton, IChildrenProvider, ISkewedComponent {
        #region UI Props

        public IColorSet? GradientColors0 {
            get => _gradientColors0;
            set {
                _gradientColors0 = value;
                RefreshGradientState();
                UpdateColor();
                NotifyPropertyChanged();
            }
        }

        public IColorSet? GradientColors1 {
            get => _gradientColors1;
            set {
                _gradientColors1 = value;
                RefreshGradientState();
                UpdateColor();
                NotifyPropertyChanged();
            }
        }

        public float Skew {
            get => _skew;
            set {
                _skew = value;
                ApplySkew(value);
            }
        }

        public new ICollection<ILayoutItem> Children => base.Children;

        private float _skew;
        private IColorSet? _gradientColors0;
        private IColorSet? _gradientColors1;

        private void RefreshGradientState() {
            Image.UseGradient = GradientColors0 != null || GradientColors1 != null;
        }

        protected virtual void ApplySkew(float skew) {
            Image.Skew = skew;
        }

        #endregion

        #region UI Components

        public Image Image { get; private set; } = null!;

        #endregion

        #region Color

        protected override void ApplyColor(Color color) {
            if (Colors != null) {
                Image.Color = color;
            }
            if (_gradientColors0 != null) {
                Image.GradientColor0 = GetColor(_gradientColors0);
            }
            if (_gradientColors1 != null) {
                Image.GradientColor1 = GetColor(_gradientColors1);
            }
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
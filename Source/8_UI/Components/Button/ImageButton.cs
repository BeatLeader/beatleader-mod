using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    /// <summary>
    /// Descriptor used for properties sync and export
    /// </summary>
    //TODO: reimplement base to remove descriptor requirement 
    public class ImageButtonParamsDescriptor {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public bool GrowOnHover { get; set; }

        [ExternalProperty, UsedImplicitly]
        public bool ColorizeOnHover { get; set; } = true;

        [ExternalProperty, UsedImplicitly]
        public float HoverLerpMul { get; set; } = 10f;

        [ExternalProperty, UsedImplicitly]
        public Color ActiveColor { get; set; }

        [ExternalProperty, UsedImplicitly]
        public Color HoverColor { get; set; } = Color.grey;

        [ExternalProperty, UsedImplicitly]
        public Color Color { get; set; }

        [ExternalProperty, UsedImplicitly]
        public RectOffset Pad { get; set; } = new();

        [ExternalProperty, UsedImplicitly]
        public Vector3 HoverScaleSum { get; set; } = new(0.2f, 0.2f, 0.2f);

        [ExternalProperty, UsedImplicitly]
        public Vector3 BaseScale { get; set; } = Vector3.one;

        #endregion
    }

    internal class ImageButton : ButtonComponentBase<ImageButton> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public bool ColorizeOnHover {
            get => _colorizeOnHover;
            set {
                _colorizeOnHover = value;
                if (!IsInitialized) return;
                UpdateColor(0);
            }
        }

        [ExternalProperty, UsedImplicitly]
        public Color ActiveColor { get; set; } = DefaultHoveredColor;

        [ExternalProperty, UsedImplicitly]
        public Color HoverColor { get; set; } = DefaultHoveredColor;

        [ExternalProperty, UsedImplicitly]
        public Color Color {
            get => _color;
            set {
                _color = value;
                UpdateColor(0);
            }
        }

        private bool _colorizeOnHover = true;
        private Color _color = DefaultColor;

        #endregion

        #region UI Components

        [ExternalComponent, UsedImplicitly]
        [ExternalProperty(prefix: null,
            nameof(AdvancedImage.Sprite),
            nameof(AdvancedImage.PreserveAspect))]
        public AdvancedImage Image { get; set; } = null!;

        #endregion

        #region Color

        public static readonly Color DefaultHoveredColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        public static readonly Color DefaultColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        private void UpdateColor(float hoverProgress) {
            hoverProgress = _colorizeOnHover ? hoverProgress : 0;
            Image.Color = IsActive ? ActiveColor : Color.Lerp(Color, HoverColor, hoverProgress);
        }

        protected override void OnHoverProgressChange(float progress) {
            UpdateColor(progress);
        }

        protected override void OnButtonStateChange(bool state) {
            UpdateColor(1);
        }

        #endregion

        #region Setup

        protected override void OnContentConstruct(Transform parent) {
            Image = AdvancedImage.Instantiate(parent);
            Image.InheritSize = true;
            Image.Material = BundleLoader.UIAdditiveGlowMaterial;
        }

        #endregion
    }
}
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class AeroButton : ImageButton {
        private static readonly Color hoveredGradientColor = Color.white.ColorWithAlpha(0.5f);
        private static readonly Color defaultGradientColor = Color.white;

        protected override void OnInteractableChange(bool interactable) {
            Image.UseGradient = interactable;
            UpdateColor();
        }

        protected override void OnInitialize() {
            Image.Sprite = BundleLoader.Sprites.background;
            Image.ImageType = UnityEngine.UI.Image.Type.Sliced;
            Image.PixelsPerUnit = 12f;
            Image.GradientDirection = ImageView.GradientDirection.Vertical;
            Image.GradientColor0 = Color.white;
            Colors = new() {
                HoveredColor = UIStyle.InputColorSet.HoveredColor.ColorWithAlpha(1f),
                Color = UIStyle.InputColorSet.Color,
                DisabledColor = UIStyle.InputColorSet.DisabledColor
            };
            GradientColors1 = new() {
                Color = defaultGradientColor,
                HoveredColor = hoveredGradientColor
            };
        }
    }
}
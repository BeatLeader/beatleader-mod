using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class BsButton : ImageButton {
        private static readonly Color inactiveColor = Color.white;
        private static readonly Color activeColor = Color.white.ColorWithAlpha(0.5f);

        protected override void ApplyColor(Color color, float t) {
            Image.GradientColor0 = Color.white;
            Image.GradientColor1 = Color.Lerp(inactiveColor, activeColor, t);
            Image.Color = color;
        }

        protected override void OnInitialize() {
            Color = Color.black.ColorWithAlpha(0.5f);
            HoverColor = Color.white.ColorWithAlpha(0.3f);
            ActiveColor = Color.white.ColorWithAlpha(0.5f);
            GrowOnHover = false;
            HoverLerpMul = 100f;
            Image.Sprite = BundleLoader.WhiteBG;
            Image.PixelsPerUnit = 15f;
            Image.UseGradient = true;
        }
    }
}
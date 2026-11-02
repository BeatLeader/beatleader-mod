using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI {
    internal static class UIStyle {
        public static float Skew => BeatSaberStyle.Skew;
        public static Color SecondaryTextColor => BeatSaberStyle.SecondaryTextColor;

        private static readonly Color PrimaryButtonColor = new(0f, 0.5f, 1f);

        public static readonly ColorSet InputColorSet = new() {
            HoveredColor = Color.magenta.ColorWithAlpha(0.5f),
            Color = Color.black.ColorWithAlpha(0.5f),
            NotInteractableColor = Color.black.ColorWithAlpha(0.2f)
        };

        public static readonly ColorSet ControlColorSet = new() {
            HoveredColor = Color.white.ColorWithAlpha(0.2f),
            Color = Color.black.ColorWithAlpha(0.5f),
            NotInteractableColor = Color.black.ColorWithAlpha(0.25f)
        };

        public static readonly ColorSet ControlButtonColorSet = new() {
            ActiveColor = new(0f, 0.75f, 1f, 1f),
            HoveredColor = Color.white.ColorWithAlpha(0.2f),
            Color = Color.black.ColorWithAlpha(0.5f),
            NotInteractableColor = Color.black.ColorWithAlpha(0.35f)
        };

        public static readonly ColorSet GlowingButtonColorSet = new() {
            ActiveColor = PrimaryButtonColor,
            HoveredColor = PrimaryButtonColor,
            Color = (Color.white * 0.8f).ColorWithAlpha(0.2f),
            NotInteractableColor = Color.clear
        };

        public static BsAeroButtonColors AeroButtonColors(ColorSet backgroundColors) {
            return BeatSaberStyle.BsAeroButtonColors with {
                BackgroundColors = BeatSaberStyle.BsAeroButtonColors.BackgroundColors with { Colors = backgroundColors }
            };
        }
    }
}
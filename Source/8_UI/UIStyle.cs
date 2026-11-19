using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI {
    internal static class UIStyle {
        private static readonly Color PrimaryButtonColor = new(0f, 0.5f, 1f);

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
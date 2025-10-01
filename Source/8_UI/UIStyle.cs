using Reactive.BeatSaber;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI {
    internal static class UIStyle {
        public static float Skew => BeatSaberStyle.Skew;
        public static Color SecondaryTextColor => BeatSaberStyle.SecondaryTextColor;

        public static SimpleColorSet InputColorSet => BeatSaberStyle.InputColorSet;
        public static SimpleColorSet ControlColorSet => BeatSaberStyle.ControlColorSet;
        public static SimpleColorSet ControlButtonColorSet => BeatSaberStyle.ControlButtonColorSet;

        public static ReadOnlyColorSet GlowingButtonColorSet => new() {
            ActiveColor = BeatSaberStyle.PrimaryButtonColor,
            HoveredColor = BeatSaberStyle.PrimaryButtonColor,
            Color = (Color.white * 0.8f).ColorWithAlpha(0.2f)
        };
    }
}
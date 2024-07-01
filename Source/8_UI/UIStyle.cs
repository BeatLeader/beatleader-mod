using BeatLeader.UI.Reactive;
using UnityEngine;

namespace BeatLeader.UI {
    internal static class UIStyle {
        public static readonly float Skew = 0.18f;

        public static ReadOnlyColorSet InputColorSet => new() {
            HoveredColor = Color.magenta.ColorWithAlpha(0.5f),
            Color = Color.black.ColorWithAlpha(0.5f),
            DisabledColor = Color.black.ColorWithAlpha(0.2f)
        };

        public static ReadOnlyColorSet ControlColorSet => new() {
            HoveredColor = Color.white.ColorWithAlpha(0.2f),
            Color = Color.black.ColorWithAlpha(0.5f)
        };

        public static ReadOnlyColorSet PrimaryButtonColorSet => new() {
            ActiveColor = new(0, 0.75f, 0.75f),
            HoveredColor = new(0, 0.75f, 1f),
            Color = new(0, 0.5f, 1f)
        };

        public static ReadOnlyColorSet ControlButtonColorSet => new() {
            ActiveColor = new(0f, 0.75f, 1f, 1f),
            HoveredColor = Color.white.ColorWithAlpha(0.2f),
            Color = Color.black.ColorWithAlpha(0.5f),
            DisabledColor = Color.black.ColorWithAlpha(0.2f)
        };

        public static ReadOnlyColorSet ButtonColorSet => new() {
            ActiveColor = new(0.0f, 0.4f, 1.0f, 1.0f),
            HoveredColor = new(0.0f, 0.4f, 1.0f, 1.0f),
            Color = (Color.white * 0.8f).ColorWithAlpha(0.2f)
        };
        
        public static StateColorSet SecondaryButtonColorSet => new() {
            ActiveColor = new(0.0f, 0.4f, 1.0f, 1.0f),
            HoveredColor = (Color.white * 0.5f).ColorWithAlpha(0.2f),
            Color = (Color.white * 0.8f).ColorWithAlpha(0.2f)
        };

        public static ReadOnlyColorSet TextColorSet => new() {
            ActiveColor = new(0f, 0.75f, 1f, 1f),
            DisabledColor = Color.white.ColorWithAlpha(0.2f),
            HoveredColor = Color.white * 0.9f,
            Color = Color.white
        };

        public static readonly Color TextColor = Color.white;
        public static readonly Color SelectedTextColor = new(0f, 0.75f, 1f, 1f);
        public static readonly Color InactiveTextColor = Color.white.ColorWithAlpha(0.2f);
        public static readonly Color SecondaryTextColor = Color.white * 0.9f;
    }
}
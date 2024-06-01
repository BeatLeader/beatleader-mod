using UnityEngine;

namespace BeatLeader.UI {
    internal static class UIStyle {
        public static readonly float Skew = 0.18f;
        
        public static readonly Color ControlHoveredColor = Color.white.ColorWithAlpha(0.2f);
        public static readonly Color ControlColor = Color.black.ColorWithAlpha(0.5f);
        
        public static readonly Color ButtonSelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        public static readonly Color ButtonHoveredColor = (Color.white * 0.5f).ColorWithAlpha(0.2f);
        public static readonly Color ButtonColor = new(0.8f, 0.8f, 0.8f, 0.2f);
        
        public static readonly Color TextColor = Color.white;
        public static readonly Color SelectedTextColor = new(0f, 0.75f, 1f, 1f);
        public static readonly Color InactiveTextColor = Color.white.ColorWithAlpha(0.2f);
        public static readonly Color SecondaryTextColor = Color.white * 0.9f;
    }
}
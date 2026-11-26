using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal static class ProfileStyle {
        public const string OpenedInBrowserText = "Opened in browser";

        public static readonly Color NeutralColor = new(0.3f, 0.6f, 1f);
        public static readonly Color AccColor = new(0.25f, 0.85f, 0.8f);
        public static readonly Color RankColor = new(1f, 0.5f, 0.75f);
        public static readonly Color PpColor = new(0.72f, 0.34f, 1f);

        public static Color ParseColor(string? hex) {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : NeutralColor;
        }
    }
}

using System.Globalization;

namespace BeatLeader {
    public static class FormatUtils {
        public static readonly NumberFormatInfo ScoreFormatInfo = new CultureInfo("en-US", false).NumberFormat;

        static FormatUtils() {
            ScoreFormatInfo.NumberGroupSeparator = " ";
        }

        public static string FormatScore(int value) {
            return value.ToString("N0", ScoreFormatInfo);
        }

        #region FormatAcc

        private const string AccColorSSPlus = "#8F48DB";
        private const string AccColorSS = "#BF2A42";
        private const string AccColorSPlus = "#FF6347";
        private const string AccColorS = "#59B0F4";
        private const string AccColorA = "#3CB371";
        private const string AccColorMeh = "#EEFF9E";

        public static string FormatAcc(float value) {
            return $"<color={GetAccColorString(value)}>{value * 100.0f:F2}<size=70%>%";
        }

        private static string GetAccColorString(float acc) {
            return acc switch {
                >= 0.95f => AccColorSSPlus,
                >= 0.9f => AccColorSS,
                >= 0.85f => AccColorSPlus,
                >= 0.8f => AccColorS,
                >= 0.7f => AccColorA,
                _ => AccColorMeh
            };
        }

        #endregion

        #region FormatPP

        private const string PPColor = "#B856FF";

        public static string FormatPP(float value) {
            return $"<color={PPColor}>{value:F2}<size=70%>pp";
        }

        #endregion
    }
}
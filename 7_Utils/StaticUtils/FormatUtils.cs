using System.Globalization;
using UnityEngine;

namespace BeatLeader {
    public static class FormatUtils {
        #region FormatRank

        public static string FormatRank(int rank, bool withPrefix) {
            return $"{(withPrefix ? "#" : "")}{rank}";
        }

        #endregion

        #region FormatUsername

        public static string FormatUserName(string userName) {
            return $"<noparse>{userName}</noparse>";
        }

        #endregion

        #region FormatScore

        public static readonly NumberFormatInfo ScoreFormatInfo = new CultureInfo("en-US", false).NumberFormat;

        static FormatUtils() {
            ScoreFormatInfo.NumberGroupSeparator = " ";
        }

        public static string FormatScore(int value) {
            return value.ToString("N0", ScoreFormatInfo);
        }

        #endregion

        #region FormatModifiers

        private const string ModifiersColor = "#999999";

        public static string FormatModifiers(string modifiers) {
            return $"<color={ModifiersColor}>{modifiers}";
        }

        #endregion

        #region FormatAcc

        private static readonly Color LowAccColor = new Color(0.93f, 1f, 0.62f);
        private static readonly Color HighAccColor = new Color(1f, 0.39f, 0.28f);

        public static string FormatAcc(float value) {
            return $"<color=#{GetAccColorString(value)}>{value * 100.0f:F2}<size=70%>%";
        }

        private static string GetAccColorString(float acc) {
            var lerpValue = Mathf.Pow(acc, 14.0f);
            var color = Color.Lerp(LowAccColor, HighAccColor, lerpValue);
            return ColorUtility.ToHtmlStringRGBA(color);
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
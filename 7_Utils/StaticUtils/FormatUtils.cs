using System;
using System.Globalization;
using System.Linq;
using BeatLeader.Models;
using ModestTree;
using UnityEngine;

namespace BeatLeader {
    internal static class FormatUtils {
        #region GetPlayerRole

        public static PlayerRole[] ParsePlayerRoles(string roles) {
            return roles.Split(',').Select(ParseSingleRole).ToArray();
        }

        public static PlayerRole ParseSingleRole(string role) {
            return role switch {
                "admin" => PlayerRole.Admin,
                "supporter" => PlayerRole.Supporter,
                _ => PlayerRole.Default
            };
        }

        #endregion

        #region GetHeadsetName

        public static string GetHeadsetNameById(int id) {
            return id switch {
                1 => "Rift",
                2 => "Vive",
                4 => "Vive Pro",
                8 => "WMR",
                16 => "Rift S",
                32 => "Quest",
                64 => "Index",
                128 => "Vive Cosmos",
                256 => "Quest 2",
                _ => "Unknown HMD"
            };
        }

        #endregion

        #region GetRelativeTimeString

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public static string GetRelativeTimeString(string timeSet) {
            var dateTime = long.Parse(timeSet).AsUnixTime();

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dateTime.Ticks);
            var delta = Math.Abs(ts.TotalSeconds);

            switch (delta) {
                case < 1 * Minute: return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
                case < 2 * Minute: return "a minute ago";
                case < 45 * Minute: return ts.Minutes + " minutes ago";
                case < 90 * Minute: return "an hour ago";
                case < 24 * Hour: return ts.Hours + " hours ago";
                case < 48 * Hour: return "yesterday";
                case < 30 * Day: return ts.Days + " days ago";
                case < 12 * Month: {
                    var months = Convert.ToInt32(Math.Floor((double) ts.Days / 30));
                    return months <= 1 ? "one month ago" : months + " months ago";
                }
                default: {
                    var years = Convert.ToInt32(Math.Floor((double) ts.Days / 365));
                    return years <= 1 ? "one year ago" : years + " years ago";
                }
            }
        }

        #endregion

        #region FormatRank

        public static string FormatRank(int rank, bool withPrefix) {
            return $"{(withPrefix ? "<size=70%>#</size>" : "")}{rank}";
        }

        #endregion

        #region FormatUsername

        public static string FormatUserName(string userName) {
            return $"<noparse>{userName}</noparse>";
        }

        #endregion

        #region FormatClanTag
        
        public static string FormatClanTag(string tag) {
            return $"<alpha=#00>.<alpha=#FF><b><noparse>{tag}</noparse></b><alpha=#00>.<alpha=#FF>";
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
            return modifiers.IsEmpty() ? "" : $"<color={ModifiersColor}>{modifiers}";
        }

        #endregion

        #region FormatAcc

        private static readonly Color LowAccColor = new Color(0.93f, 1f, 0.62f);
        private static readonly Color HighAccColor = new Color(1f, 0.39f, 0.28f);

        public static string FormatAcc(float value) {
            return $"<color=#{GetAccColorString(value)}>{value * 100.0f:F2}<size=70%>%</size>";
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
            return $"<color={PPColor}>{value:F2}<size=70%>pp</size>";
        }

        #endregion
    }
}
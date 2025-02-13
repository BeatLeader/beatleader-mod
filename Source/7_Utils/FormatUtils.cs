using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BeatLeader.Models;
using ModestTree;
using UnityEngine;

namespace BeatLeader {
    internal static class FormatUtils {
        #region Platform

        public static string GetFullPlatformName(string serverPlatform) {
            return serverPlatform switch {
                "oculus" => "Oculus Standalone",
                "oculuspc" => "Oculus PC",
                "steam" => "Steam",
                _ => serverPlatform
            };
        }

        #endregion

        #region RankedStatus

        public static readonly IReadOnlyList<RankedStatus> NegativeModifiersAppliers = new RankedStatus[] {
            RankedStatus.Nominated,
            RankedStatus.Qualified,
            RankedStatus.Ranked
        };

        public static RankedStatus GetRankedStatus(DiffInfo diffInfo) {
            return diffInfo.status switch {
                0 => RankedStatus.Unranked,
                1 => RankedStatus.Nominated,
                2 => RankedStatus.Qualified,
                3 => RankedStatus.Ranked,
                4 => RankedStatus.Unrankable,
                5 => RankedStatus.Outdated,
                6 => RankedStatus.Event,
                7 => RankedStatus.OST,
                _ => RankedStatus.Unknown
            };
        }

        #endregion

        #region GetPlayerRole

        public static bool IsAnyAdmin(this PlayerRole playerRole) => playerRole is PlayerRole.Admin;
        public static bool IsAnyRT(this PlayerRole playerRole) => playerRole is PlayerRole.RankedTeam or PlayerRole.JuniorRankedTeam;
        public static bool IsAnySupporter(this PlayerRole playerRole) => playerRole is PlayerRole.Tipper or PlayerRole.Supporter or PlayerRole.Sponsor;

        public static PlayerRole GetSupporterRole(PlayerRole[] playerRoles) {
            foreach (var playerRole in playerRoles) {
                switch (playerRole) {
                    case PlayerRole.Tipper:
                    case PlayerRole.Supporter:
                    case PlayerRole.Sponsor: return playerRole;
                    default: continue;
                }
            }

            return PlayerRole.Default;
        }

        public static PlayerRole[] ParsePlayerRoles(string roles) {
            return roles.Split(',').Select(ParseSingleRole).ToArray();
        }

        public static PlayerRole ParseSingleRole(string role) {
            return role switch {
                "admin" => PlayerRole.Admin,
                "rankedteam" => PlayerRole.RankedTeam,
                "juniorrankedteam" => PlayerRole.JuniorRankedTeam,
                "mapper" => PlayerRole.Mapper,
                "creator" => PlayerRole.Creator,
                "tipper" => PlayerRole.Tipper,
                "supporter" => PlayerRole.Supporter,
                "sponsor" => PlayerRole.Sponsor,
                _ => PlayerRole.Default
            };
        }

        #endregion

        #region DiffForName

        public static int DiffIdForDiffName(string diffName) {
            switch (diffName) {
                case "Easy":
                case "easy":
                    return 1;
                case "Normal":
                case "normal":
                    return 3;
                case "Hard":
                case "hard":
                    return 5;
                case "Expert":
                case "expert":
                    return 7;
                case "ExpertPlus":
                case "expertPlus":
                    return 9;
            }

            return 0;
        }

        #endregion

        #region FormatTimeset

        private static readonly Color OldScoreColor = new(0.5f, 0.5f, 0.5f);
        private static readonly Color FreshScoreColor = new(1f, 1f, 1f);
        private static readonly Range ScoreLifetimeDaysRange = new(0, 30 * 8);

        public static string FormatTimeset(string timeSet, bool compact) {
            var timeSpan = GetRelativeTime(timeSet);
            var timeString = GetRelativeTimeString(timeSpan, compact);
            return $"<color=#{GetTimesetColorString(timeSpan)}>{timeString}</color>";
        }

        private static string GetTimesetColorString(TimeSpan timeSpan) {
            var lerpValue = Mathf.Pow(1 - ScoreLifetimeDaysRange.GetRatioClamped(timeSpan.Days), 3.0f);
            var color = Color.Lerp(OldScoreColor, FreshScoreColor, lerpValue);
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        #endregion

        #region GetRelativeTimeString

        public static TimeSpan GetRelativeTime(string timeSet) {
            var dateTime = long.Parse(timeSet).AsUnixTime();
            return DateTime.UtcNow - dateTime;
        }

        public static string GetRelativeTimeString(string timeSet, bool compact) {
            return GetRelativeTimeString(GetRelativeTime(timeSet), compact);
        }

        public static string GetDateTimeString(long timestamp) {
            var t = timestamp.AsUnixTime().ToLocalTime();
            return $"{t.Year}.{Zero(t.Month)}{t.Month}.{Zero(t.Day)}{t.Day} {Zero(t.Hour)}{t.Hour}:{Zero(t.Minute)}{t.Minute}";
            static string Zero(int number) => number > 9 ? "" : "0";
        }

        public static string GetDateTimeString(string timeSet) {
            return GetDateTimeString(long.Parse(timeSet));
        }

        public static string GetRelativeTimeString(TimeSpan timeSpan, bool compact) {
            return TimeLocalizationUtils.GetRelativeTimeStringLocalizedWithFont(timeSpan, compact);
        }

        #endregion

        #region WrapPhrase

        public static string MarkPhrase(string text, string phrase) {
            return MarkPhrase(text, phrase, new(0.8f, 0f, 1f, 1f), true);
        }

        public static string MarkPhrase(string text, string phrase, Color color, bool bold) {
            return WrapPhrase(text, phrase, $"{(bold ? "<b>" : "")}<color=#{ColorUtility.ToHtmlStringRGBA(color)}>", $"</color>{(bold ? "</b>" : "")}");
        }

        public static string WrapPhrase(string text, string phrase, string before, string after) {
            if (string.IsNullOrEmpty(phrase)) return text;
            var startIndex = 0;
            while (startIndex != -1) {
                startIndex = WrapPhraseSingle(startIndex, ref text, phrase, before, after);
            }
            return text;
        }

        private static int WrapPhraseSingle(int startIndex, ref string text, string phrase, string before, string after) {
            var startIdx = text.IndexOf(phrase, startIndex, StringComparison.OrdinalIgnoreCase);
            if (startIdx == -1) return -1;
            var endIdx = startIdx + phrase.Length;
            text = text.Insert(endIdx, after).Insert(startIdx, before);
            return endIdx + after.Length + before.Length;
        }

        #endregion

        #region FormatTime

        public static string FormatTime(float seconds) {
            return FormatTime(Mathf.FloorToInt(seconds));
        }
        
        public static string FormatTime(int seconds) {
            var minutes = seconds / 60;
            var hours = minutes / 60;
            var secDiv = seconds % 60;
            var minDiv = minutes % 60;
            return $"{(hours is not 0 ? $"{Zero(hours)}{hours}:" : "")}{Zero(minDiv)}{minDiv}:{Zero(secDiv)}{secDiv}";
            static string Zero(int number) => number > 9 ? "" : "0";
        }

        #endregion
        
        #region FormatSongTime

        public static string FormatSongTime(float time, float totalTime) {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            var totalMinutes = Mathf.FloorToInt(totalTime / 60);
            var totalSeconds = Mathf.FloorToInt(totalTime % 60);
            return $"{minutes}:{seconds:0#}/{totalMinutes}:{totalSeconds:0#}";
        }

        #endregion

        #region FormatRank

        public static string FormatRank(int rank, bool withPrefix) {
            return $"{(withPrefix ? "<size=70%>#</size>" : "")}{(rank is -1 ? "?" : rank)}";
        }

        #endregion

        #region FormatUsername

        public static string FormatUserName(string userName) {
            return userName;
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
            return value == 0f ? "<size=90%>Not Ready" : $"<color=#{GetAccColorString(value)}>{value * 100.0f:F2}<size=70%>%</size>";
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
            return $"<color={PPColor}>" + (value is -1 ? "?" : $"{value:F2}<size=70%>pp</size>");
        }

        #endregion

        #region FormatStars

        public static string FormatStars(float value) {
            return $"{value:f2}<size=70%>★</size>";
        }

        #endregion

        #region FormatPauses

        public static string FormatPauses(int value) {
            return $"{value}<size=50%> <b><color=#999999>ll";
        }

        #endregion

        #region FormatLocation

        public static string FormatLocation(UnityEngine.Vector3 location, float? rotation = null, int dimensions = 3) {
            static double round(float t) => Math.Round(t, 2);
            var line = $"<color=\"green\">X:{round(location.x)} ";
            line += dimensions >= 2 ? $"<color=\"red\">Y:{round(location.y)} " : string.Empty;
            line += dimensions >= 3 ? $"<color=\"blue\">Z:{round(location.z)} " : string.Empty;
            line += rotation != null ? $"<color=\"yellow\">R:{rotation}�" : string.Empty;
            return line;
        }

        #endregion

        #region TruncateEllipsis

        public static string TruncateEllipsis(string str, int threshold) {
            if (str.Length <= threshold) {
                return str;
            }
            return str.Substring(0, threshold - 3) + "...";
        }

        #endregion
    }
}
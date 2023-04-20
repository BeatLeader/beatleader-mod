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

        public static readonly IReadOnlyList<RankedStatus> NegativeModifiersAppliers = new RankedStatus[]
        {
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
                65 => "Controllable",
                128 => "Vive Cosmos",
                256 => "Quest 2",
                _ => "Unknown HMD"
            };
        }

        #endregion

        #region GetControllerName

        public static string GetControllerNameById(int id) {
            return id switch {
                0 => "Unknown",
                1 => "Oculus Touch",
                16 => "Oculus Touch 2",
                256 => "Oculus Touch", //Quest 2
                2 => "Vive Wands", //Vive
                4 => "Vive Pro Wands", //Vive Pro
                8 => "WMR Controllers", //WMR
                9 => "Odyssey Controllers", //Odyssey
                10 => "HP Motion",
                33 => "Pico Neo 3 Controllers", //Pico Neo 3
                34 => "Pico Neo 2 Controllers", //Pico Neo 2
                35 => "Vive Pro 2 Wands", //Vive Pro 2
                37 => "Miramar",
                44 => "Disco",
                61 => "Touch Pro", //Quest Pro
                62 => "Vive Tracker",
                63 => "Vive Tracker 2",
                64 => "Knuckles",
                65 => "Nolo",
                66 => "Pico Phoenix",
                67 => "Hands",
                68 => "Vive Tracker 3",
                69 => "Pimax",
                70 => "Huawei",
                71 => "Polaris",
                72 => "Tundra",
                73 => "Cry",
                74 => "E4",
                75 => "Gamepad",
                76 => "Joy-Con",
                77 => "Steam Deck",
                128 => "Cosmos Controllers",
                _ => "Unknown"
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

        public static string FormatTimeset(string timeSet) {
            var timeSpan = GetRelativeTime(timeSet);
            var timeString = GetRelativeTimeString(timeSpan);
            return $"<color=#{GetTimesetColorString(timeSpan)}>{timeString}</color>";
        }

        private static string GetTimesetColorString(TimeSpan timeSpan) {
            var lerpValue = Mathf.Pow(1 - ScoreLifetimeDaysRange.GetRatioClamped(timeSpan.Days), 3.0f);
            var color = Color.Lerp(OldScoreColor, FreshScoreColor, lerpValue);
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        #endregion

        #region GetRelativeTimeString

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public static TimeSpan GetRelativeTime(string timeSet) {
            var dateTime = long.Parse(timeSet).AsUnixTime();
            return DateTime.UtcNow - dateTime;
        }

        public static string GetRelativeTimeString(string timeSet) {
            return GetRelativeTimeString(GetRelativeTime(timeSet));
        }

        public static string GetRelativeTimeString(TimeSpan timeSpan) {
            switch (timeSpan.TotalSeconds) {
                case < 0: return "-";
                case < 1 * Minute: return timeSpan.Seconds == 1 ? "1 second ago" : timeSpan.Seconds + " seconds ago";
                case < 2 * Minute: return "1 minute ago";
                case < 1 * Hour: return timeSpan.Minutes + " minutes ago";
                case < 2 * Hour: return "1 hour ago";
                case < 24 * Hour: return timeSpan.Hours + " hours ago";
                case < 2 * Day: return "yesterday";
                case < 30 * Day: return timeSpan.Days + " days ago";
                case < 12 * Month: {
                    var months = Convert.ToInt32(Math.Floor((double) timeSpan.Days / 30));
                    return months <= 1 ? "1 month ago" : months + " months ago";
                }
                default: {
                    var years = Convert.ToInt32(Math.Floor((double) timeSpan.Days / 365));
                    return years <= 1 ? "1 year ago" : years + " years ago";
                }
            }
        }

        #endregion

        #region FormatSongTime

        public static string FormatSongTime(float time, float totalTime)
        {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time % 60);
            var totalMinutes = Mathf.FloorToInt(totalTime / 60);
            var totalSeconds = Mathf.FloorToInt(totalTime % 60);
            return $"{minutes}:{seconds:0#}/{totalMinutes}:{totalSeconds:0#}";
        }

        #endregion

        #region FormatRank

        public static string FormatRank(int rank, bool withPrefix) {
            return $"{(withPrefix ? "<size=70%>#</size>" : "")}{rank}";
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
    }
}
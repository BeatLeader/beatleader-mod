using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using BeatLeader.Utils;
using IPA.Loader;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.API {
    internal static class NetworkingUtils {

        public static readonly JsonSerializerSettings SerializerSettings = new() {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static void GetRequestFailReason(HttpResponseMessage httpResponse, out string failReason, out bool shouldRetry) {
            var reasonPhrase = httpResponse.ReasonPhrase.Length > 0 ? httpResponse.ReasonPhrase : null;
            var responseCode = (int)httpResponse.StatusCode;

            switch (responseCode) {
                case BLConstants.MaintenanceStatus: {
                    failReason = reasonPhrase ?? "Maintenance";
                    shouldRetry = false;
                    break;
                }
                case BLConstants.OutdatedModStatus: {
                    failReason = reasonPhrase ?? "Mod update required";
                    shouldRetry = false;
                    break;
                }
                case BLConstants.Unauthorized: {
                    Authentication.ResetLogin();
                    failReason = "Auth failed";
                    shouldRetry = true;
                    break;
                }
                default: {
                    failReason = reasonPhrase ?? $"Http error: {responseCode}";
                    shouldRetry = responseCode is < 400 or >= 500;
                    break;
                }
            }
        }

        #region Utils

        public static void BeatmapKeyToUrlParams(
            in BeatmapKey beatmapKey,
            out string mapHash, out string mapDiff, out string mapMode
        ) {
            mapHash = beatmapKey.levelId.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            mapDiff = beatmapKey.difficulty.ToString();
            mapMode = beatmapKey.beatmapCharacteristic.serializedName;
        }

        public static string ToHttpParams(Dictionary<string, object> param) {
            if (param.Count == 0) return "";

            StringBuilder sb = new();

            foreach (var item in param) {
                if (sb.Length > 0) sb.Append("&");
                sb.Append($"{item.Key}={ArgToString(item.Value)}");
            }

            return sb.ToString();
        }

        private static string ArgToString(object o) {
            return Convert.ToString(o, pointCulture);
        }

        private static readonly CultureInfo pointCulture = new("en") {
            NumberFormat = {
                NumberGroupSeparator = "",
                NumberDecimalSeparator = ".",
                PercentGroupSeparator = "",
                PercentDecimalSeparator = "."
            }
        };

        #endregion
    }
}

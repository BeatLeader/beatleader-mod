using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BeatLeader.API;
using BeatLeader.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.Utils {
    internal static class HttpUtils {
        #region GetBytes
        
        internal static IEnumerator GetBytes(string url, Action<byte[]> onSuccess, Action<string> onFail, int retry = 1) {
            Plugin.Log.Debug($"Request url = {url}");

            var failReason = "";
            for (var i = 1; i <= retry; i++) {
                var authHelper = new AuthHelper();
                yield return authHelper.EnsureLoggedIn();
                if (!authHelper.CheckStatus(out failReason)) continue;

                var request = new UnityWebRequest(url) {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 30
                };

                yield return request.SendWebRequest();
                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                if (request.isHttpError || request.isNetworkError) {
                    Plugin.Log.Debug($"Request failed: {request.error}");
                    GetRequestFailReason(request.responseCode, null, out failReason, out var shouldRetry);
                    if (!shouldRetry) break;
                    continue;
                }

                try {
                    onSuccess.Invoke(request.downloadHandler.data);
                    yield break;
                } catch (Exception e) {
                    Plugin.Log.Debug(e);
                    failReason = e.Message;
                }
            }
            onFail.Invoke(failReason);
        }

        #endregion
        
        #region Get single entity

        internal static IEnumerator GetData<T>(string url, Action<T> onSuccess, Action<string> onFail, int retry = 1) {
            Plugin.Log.Debug($"Request url = {url}");

            var failReason = "";
            for (int i = 1; i <= retry; i++) {
                var authHelper = new AuthHelper();
                yield return authHelper.EnsureLoggedIn();
                if (!authHelper.CheckStatus(out failReason)) continue;

                var request = new UnityWebRequest(url) {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 30
                };

                yield return request.SendWebRequest();
                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                if (request.isHttpError || request.isNetworkError) {
                    Plugin.Log.Debug($"Request failed: {request.error}");
                    GetRequestFailReason(request.responseCode, null, out failReason, out var shouldRetry);
                    if (!shouldRetry) break;
                    continue;
                }

                try {
                    onSuccess.Invoke(DeserializeResponse<T>(request));
                    yield break;
                } catch (Exception e) {
                    Plugin.Log.Debug(e);
                    failReason = e.Message;
                }
            }
            onFail.Invoke(failReason);
        }

        #endregion

        #region get list of entities

        internal static IEnumerator GetPagedData<T>(string url, Action<Paged<T>> onSuccess, Action<string> onFail, int retry = 1) {
            var uri = new Uri(url);
            Plugin.Log.Debug($"Request url = {uri}");

            var failReason = "";
            for (int i = 1; i <= retry; i++) {
                var authHelper = new AuthHelper();
                yield return authHelper.EnsureLoggedIn();
                if (!authHelper.CheckStatus(out failReason)) continue;

                var request = new UnityWebRequest(url) {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 30
                };

                yield return request.SendWebRequest();
                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                if (request.isHttpError || request.isNetworkError) {
                    GetRequestFailReason(request.responseCode, null, out failReason, out var shouldRetry);
                    if (!shouldRetry) break;
                    continue;
                }

                try {
                    onSuccess.Invoke(DeserializeResponse<Paged<T>>(request));
                    yield break;
                } catch (Exception e) {
                    Plugin.Log.Debug(e);
                    failReason = e.Message;
                }
            }
            onFail.Invoke(failReason);
        }

        #endregion

        #region GetOculusUser

        public static IEnumerator GetOculusUser(Action<OculusUserInfo> onSuccess, Action<string> onFail, int retry = 1) {
            Plugin.Log.Debug("GetOculusUser");
            
            var failReason = "";
            for (int i = 1; i <= retry; i++) {
                var ticketTask = Authentication.OculusTicket();
                yield return new WaitUntil(() => ticketTask.IsCompleted);
                
                var authToken = ticketTask.Result;
                if (authToken == null) {
                    failReason = "Authentication failed";
                    continue; // auth failed, retry
                }

                var request = new UnityWebRequest(string.Format(BLConstants.OCULUS_USER_INFO, authToken)) {
                    downloadHandler = new DownloadHandlerBuffer(),
                    timeout = 30
                };

                yield return request.SendWebRequest();
                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                if (request.isHttpError || request.isNetworkError) {
                    Plugin.Log.Debug($"Request failed: {request.error}");
                    GetRequestFailReason(request.responseCode, null, out failReason, out var shouldRetry);
                    if (!shouldRetry) break;
                    continue;
                }

                try {
                    onSuccess.Invoke(DeserializeResponse<OculusUserInfo>(request));
                    yield break;
                } catch (Exception e) {
                    Plugin.Log.Debug(e);
                    failReason = e.Message;
                }
            }
            onFail.Invoke(failReason);
        }

        #endregion

        #region Utils

        public class AuthHelper {
            public bool IsLoggedIn;
            public string FailReason = "";

            public IEnumerator EnsureLoggedIn() {
                yield return Authentication.EnsureLoggedIn(OnSuccess, OnFail);
            }

            private void OnFail(string reason) => FailReason = reason;

            private void OnSuccess() => IsLoggedIn = true;

            public bool CheckStatus(out string failReason) {
                failReason = FailReason;
                return IsLoggedIn;
            }
        }

        private static T DeserializeResponse<T>(UnityWebRequest request) {
            var body = Encoding.UTF8.GetString(request.downloadHandler.data);
            var options = new JsonSerializerSettings() {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(body, options);
        }

        internal static void GetRequestFailReason(long responseCode, [CanBeNull] string defaultReason, out string failReason, out bool shouldRetry) {
            switch (responseCode) {
                case BLConstants.MaintenanceStatus:
                {
                    failReason = "Maintenance";
                    shouldRetry = false;
                    break;
                }
                case BLConstants.OutdatedModStatus:
                {
                    failReason = "Mod update required";
                    shouldRetry = false;
                    break;
                }
                default:
                {
                    failReason = defaultReason ?? $"Network error ({responseCode})";
                    shouldRetry = responseCode is < 400 or >= 500;
                    break;
                }
            }
        }

        internal static string ToHttpParams(Dictionary<string, System.Object> param) {
            if (param.Count == 0) { return ""; }

            StringBuilder sb = new();

            foreach (var item in param) {
                if (sb.Length > 0) {
                    sb.Append("&");
                }
                sb.Append($"{item.Key}={item.Value}");
            }
            return sb.ToString();
        }

        #endregion
    }
}
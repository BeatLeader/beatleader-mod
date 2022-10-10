using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.API {
    internal static class NetworkingUtils {
        #region Constants

        public static readonly JsonSerializerSettings SerializerSettings = new() {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        #endregion

        #region SimpleRequestCoroutine

        public static IEnumerator SimpleRequestCoroutine<T>(
            IWebRequestDescriptor<T> requestDescriptor,
            Action<T> onSuccess, Action<string> onFail,
            int retries = 1,
            int timeoutSeconds = 30
        ) {
            var handler = new SimpleRequestHandler<T>(onSuccess, onFail);
            yield return ProcessRequestCoroutine(requestDescriptor, handler, retries, timeoutSeconds);
        }

        #endregion

        #region ProcessRequestCoroutine

        public static IEnumerator ProcessRequestCoroutine<T>(
            IWebRequestDescriptor<T> requestDescriptor,
            IWebRequestHandler<T> requestHandler,
            int retries = 1,
            int timeoutSeconds = 30
        ) {
            for (var i = 1; i <= retries; i++) {
                requestHandler.OnRequestStarted();

                var authHelper = new AuthHelper();
                yield return authHelper.EnsureLoggedIn();

                if (!authHelper.IsLoggedIn) {
                    requestHandler.OnRequestFailed($"Auth failed ({authHelper.FailReason})");
                    continue; //retry
                }

                var request = requestDescriptor.CreateWebRequest();
                request.timeout = timeoutSeconds;

                Plugin.Log.Debug($"Request[{request.GetHashCode()}]: {request.url}");
                yield return AwaitRequestWithProgress(request, requestHandler);
                Plugin.Log.Debug($"Response[{request.GetHashCode()}]: {request.error ?? request.responseCode.ToString()}");

                if (request.isNetworkError) {
                    requestHandler.OnRequestFailed($"Network error: {request.error} {request.downloadHandler?.text}");
                    continue; //retry
                }

                if (request.isHttpError) {
                    GetRequestFailReason(request.responseCode, request.downloadHandler?.text, out var failReason, out var shouldRetry);
                    requestHandler.OnRequestFailed(failReason);
                    if (shouldRetry) continue; //retry
                    break; //no retry
                }

                try {
                    requestHandler.OnRequestFinished(requestDescriptor.ParseResponse(request));
                    break; //no retry
                } catch (Exception e) {
                    Plugin.Log.Debug($"Response[{request.GetHashCode()}] Exception: {e}");
                    requestHandler.OnRequestFailed($"Internal error: {e.Message}");
                    break; //no retry
                }
            }
        }

        private static IEnumerator AwaitRequestWithProgress<T>(UnityWebRequest request, IWebRequestHandler<T> requestHandler) {
            var asyncOperation = request.SendWebRequest();

            var overallProgress = 0f;
            requestHandler.OnProgress(0, 0, 0);

            bool WaitUntilPredicate() => request.isDone || !overallProgress.Equals(asyncOperation.progress);

            do {
                yield return new WaitUntil(WaitUntilPredicate);
                if (overallProgress.Equals(asyncOperation.progress)) continue;
                overallProgress = asyncOperation.progress;
                requestHandler.OnProgress(request.uploadProgress, request.downloadProgress, overallProgress);
            } while (!request.isDone);
        }

        private static void GetRequestFailReason(long responseCode, [CanBeNull] string defaultReason, out string failReason, out bool shouldRetry) {
            switch (responseCode) {
                case BLConstants.MaintenanceStatus: {
                    failReason = "Maintenance";
                    shouldRetry = false;
                    break;
                }
                case BLConstants.OutdatedModStatus: {
                    failReason = "Mod update required";
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
                    failReason = defaultReason ?? $"Http error: {responseCode}";
                    shouldRetry = responseCode is < 400 or >= 500;
                    break;
                }
            }
        }

        #endregion

        #region Utils

        private class AuthHelper {
            public bool IsLoggedIn;
            public string FailReason = "";

            public IEnumerator EnsureLoggedIn() {
                yield return Authentication.EnsureLoggedIn(
                    () => IsLoggedIn = true,
                    reason => FailReason = reason
                );
            }
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

using System;
using System.Collections;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.API {
    internal static class NetworkingUtils {
        #region SimpleRequestCoroutine

        public static IEnumerator SimpleRequestCoroutine<T>(
            IWebRequestDescriptor<T> requestDescriptor,
            Action<T> onSuccess, Action<string> onFail,
            int retries = 1
        ) {
            var handler = new SimpleRequestHandler<T>(onSuccess, onFail);
            yield return ProcessRequestCoroutine(requestDescriptor, handler, retries);
        }

        #endregion

        #region ProcessRequestCoroutine

        public static IEnumerator ProcessRequestCoroutine<T>(
            IWebRequestDescriptor<T> requestDescriptor,
            IWebRequestHandler<T> requestHandler,
            int retries = 1
        ) {
            for (var i = 1; i <= retries; i++) {
                requestHandler.OnRequestStarted();

                var authHelper = new HttpUtils.AuthHelper();
                yield return authHelper.EnsureLoggedIn();

                if (!authHelper.IsLoggedIn) {
                    requestHandler.OnRequestFailed($"Auth failed ({authHelper.FailReason})");
                    continue; //retry
                }

                var request = requestDescriptor.CreateWebRequest();
                request.timeout = 30;
                
                Plugin.Log.Debug($"Request[{request.GetHashCode()}]: {request.url}");
                yield return AwaitRequestWithProgress(request, requestHandler);
                Plugin.Log.Debug($"Response[{request.GetHashCode()}]: {request.error ?? request.responseCode.ToString()}");

                if (request.isNetworkError) {
                    requestHandler.OnRequestFailed($"Network error: {request.error}");
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
                    requestHandler.OnRequestFailed($"Internal error: {e.Message}");
                    break; //no retry
                }
            }
        }

        private static IEnumerator AwaitRequestWithProgress<T>(UnityWebRequest request, IWebRequestHandler<T> requestHandler) {
            var asyncOperation = request.SendWebRequest();

            var uploadProgress = 0f;
            var downloadProgress = 0f;
            var progress = 0f;

            bool WaitUntilPredicate() => request.isDone || !progress.Equals(asyncOperation.progress);

            do {
                yield return new WaitUntil(WaitUntilPredicate);

                if (!uploadProgress.Equals(request.uploadProgress)) {
                    uploadProgress = request.uploadProgress;
                    requestHandler.OnUploadProgress(uploadProgress);
                }

                if (!downloadProgress.Equals(request.downloadProgress)) {
                    downloadProgress = request.downloadProgress;
                    requestHandler.OnDownloadProgress(downloadProgress);
                }

                if (!progress.Equals(asyncOperation.progress)) {
                    progress = asyncOperation.progress;
                    requestHandler.OnProgress(progress);
                }
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
                default: {
                    failReason = defaultReason ?? $"Http error: {responseCode}";
                    shouldRetry = responseCode is < 400 or >= 500;
                    break;
                }
            }
        }

        #endregion
    }
}
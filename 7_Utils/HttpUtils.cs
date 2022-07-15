using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace BeatLeader.Utils {
    internal static class HttpUtils {
        #region GetBytes
        
        internal static IEnumerator GetBytes(string url, Action<byte[]> onSuccess, Action<string> onFail, int retry = 1) {
            Plugin.Log.Debug($"Request url = {url}");

            var failReason = "";
            for (var i = 1; i <= retry; i++) {
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

        #region ReplayUpload

        public static IEnumerator UploadReplay(Replay replay, int retry = 3) {
            MemoryStream stream = new();
            using (var compressedStream = new GZipStream(stream, CompressionLevel.Optimal)) {
                ReplayEncoder.Encode(replay, new BinaryWriter(compressedStream, Encoding.UTF8));
            }
            var compressedData = stream.ToArray();

            for (int i = 1; i <= retry; i++) {
                string GetFailMessage(string reason) => $"Attempt {i}/{retry} failed! {reason}";
                
                LeaderboardState.UploadRequest.NotifyStarted();

                Task<string> ticketTask = Authentication.SteamTicket();
                yield return new WaitUntil(() => ticketTask.IsCompleted);

                string authToken = ticketTask.Result;
                if (authToken == null) {
                    Plugin.Log.Debug("No auth token, skip replay upload");
                    LeaderboardState.UploadRequest.NotifyFailed("Auth failed");
                    break; // auth failed, no upload
                }

                Plugin.Log.Debug($"Attempt to upload replay {i}/{retry}");

                var request = new UnityWebRequest(BLConstants.REPLAY_UPLOAD_URL + "?ticket=" + authToken, UnityWebRequest.kHttpVerbPOST) {
                    downloadHandler = new DownloadHandlerBuffer(),
                    uploadHandler = new UploadHandlerRaw(compressedData),
                };
                request.SetRequestHeader("Content-Encoding", "gzip");
                yield return request.SendWebRequest();

                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                var body = Encoding.UTF8.GetString(request.downloadHandler.data);
                if (body.Length > 0 && !(body.StartsWith("{") || body.StartsWith("[") || body.StartsWith("<"))) {
                    Plugin.Log.Debug($"Response content: {body}");
                }

                if (request.isNetworkError || request.isHttpError) {
                    Plugin.Log.Debug($"Upload failed: {request.error}");
                    GetRequestFailReason(request.responseCode, body, out var failReason, out var shouldRetry);
                    LeaderboardState.UploadRequest.NotifyFailed(GetFailMessage(failReason));
                    if (!shouldRetry) yield break;
                    continue;
                }

                try {
                    Plugin.Log.Debug(body);
                    var options = new JsonSerializerSettings() {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    Score score = JsonConvert.DeserializeObject<Score>(body, options);
                    Plugin.Log.Debug("Upload success");

                    LeaderboardState.UploadRequest.NotifyFinished(score);

                    yield break; // if OK - stop retry cycle
                } catch (Exception e) {
                    Plugin.Log.Debug("Exception");
                    Plugin.Log.Debug(e);
                    LeaderboardState.UploadRequest.NotifyFailed(GetFailMessage($"Internal error: {e.Message}"));
                }
            }
            Plugin.Log.Debug("Cannot upload replay");
        }

        #endregion

        #region Vote

        public static IEnumerator VoteCoroutine(
            string mapHash,
            string mapDiff,
            string mapMode,
            Vote vote,
            int retry = 1
        ) {
            var failReason = "";

            for (var i = 1; i <= retry; i++) {
                Plugin.Log.Debug($"Vote request: {i + 1}/{retry}");
                LeaderboardState.VoteRequest.NotifyStarted();

                var ticketTask = Authentication.SteamTicket();
                yield return new WaitUntil(() => ticketTask.IsCompleted);
                
                var authToken = ticketTask.Result;
                if (authToken == null) {
                    failReason = "Authentication failed";
                    break; // auth failed, no retries
                }

                var request = BuildVoteRequest(mapHash, mapDiff, mapMode, vote, authToken);
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError) {
                    GetRequestFailReason(request.responseCode, null, out failReason, out var shouldRetry);
                    if (!shouldRetry) break;
                    continue;
                }

                try {
                    Plugin.Log.Debug("Vote success");
                    LeaderboardState.VoteRequest.NotifyFinished(DeserializeResponse<VoteStatus>(request));
                    yield break; // if OK - stop retry cycle
                } catch (Exception e) {
                    Plugin.Log.Debug($"Exception: {e}");
                    failReason = $"Internal error: {e.Message}";
                }
            }

            Plugin.Log.Debug($"Vote failed: {failReason}");
            LeaderboardState.VoteRequest.NotifyFailed(failReason);
        }

        private static UnityWebRequest BuildVoteRequest(string mapHash, string mapDiff, string mapMode, Vote vote, string authToken) {
            var query = new Dictionary<string, object> {
                ["rankability"] = vote.Rankability,
                ["ticket"] = authToken
            };
            if (vote.HasStarRating) query["stars"] = vote.StarRating;
            if (vote.HasMapType) query["type"] = (int) vote.MapType;
            var url = string.Format(BLConstants.VOTE, mapHash, mapDiff, mapMode, ToHttpParams(query));
            return new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST) {
                downloadHandler = new DownloadHandlerBuffer()
            };
        }

        #endregion

        #region GetOculusUser

        public static IEnumerator GetOculusUser(Action<OculusUserInfo> onSuccess, Action<string> onFail, int retry = 1) {
            Plugin.Log.Debug("GetOculusUser");
            
            var failReason = "";
            for (int i = 1; i <= retry; i++) {
                var ticketTask = Authentication.SteamTicket();
                yield return new WaitUntil(() => ticketTask.IsCompleted);
                
                var authToken = ticketTask.Result;
                if (authToken == null) {
                    failReason = "Authentication failed";
                    break; // auth failed, no retries
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
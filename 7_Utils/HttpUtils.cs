using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace BeatLeader.Utils {
    internal static class HttpUtils {
        #region Get single entity

        internal static IEnumerator GetData<T>(string url, Action<T> onSuccess, Action<string> onFail, int retry = 1) {
            Plugin.Log.Debug($"Request url = {url}");

            var failReason = "";
            for (int i = 1; i <= retry; i++) {
                void StopRetries() => i = retry;

                var handler = new DownloadHandlerBuffer();

                var request = new UnityWebRequest(url) {
                    downloadHandler = handler,
                    timeout = 30
                };

                yield return request.SendWebRequest();
                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                if (request.isHttpError || request.isNetworkError) {
                    Plugin.Log.Debug($"Request failed: {request.error}");
                    switch(request.responseCode) {
                        case BLConstants.MaintenanceStatus: {
                            failReason = "Maintenance";
                            StopRetries();
                            break;
                        }
                        case BLConstants.OutdatedModStatus: {
                            failReason = "Mod update required";
                            StopRetries();
                            break;
                        }
                        default: {
                            failReason = $"Connection error ({request.responseCode})";
                            break;
                        }
                    };
                    continue;
                }

                try {
                    var options = new JsonSerializerSettings() {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    var result = JsonConvert.DeserializeObject<T>(handler.text, options);
                    onSuccess.Invoke(result);
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

                var handler = new DownloadHandlerBuffer();
                var request = new UnityWebRequest(url) {
                    downloadHandler = handler,
                    timeout = 30
                };

                yield return request.SendWebRequest();
                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                if (request.isHttpError || request.isNetworkError) {
                    continue;
                }

                try {
                    var options = new JsonSerializerSettings() {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    var result = JsonConvert.DeserializeObject<Paged<T>>(handler.text, options);
                    onSuccess.Invoke(result);
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

                try {
                    if (request.isNetworkError || request.isHttpError) {
                        Plugin.Log.Debug($"Upload failed: {request.error}");
                        switch (request.responseCode) {
                            case 400:
                                LeaderboardState.UploadRequest.NotifyFailed(body);
                                yield break; //no PB, stop retry cycle
                            default:
                                LeaderboardState.UploadRequest.NotifyFailed(GetFailMessage($"Network error: {request.responseCode}"));
                                break;
                        }
                    } else {
                        Plugin.Log.Debug(body);
                        var options = new JsonSerializerSettings() {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        Score score = JsonConvert.DeserializeObject<Score>(body, options);
                        Plugin.Log.Debug("Upload success");

                        LeaderboardState.UploadRequest.NotifyFinished(score);

                        yield break; // if OK - stop retry cycle
                    }
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
                    failReason = $"Network error: {request.responseCode}";
                    continue;
                }

                VoteStatus status;

                try {
                    var body = Encoding.UTF8.GetString(request.downloadHandler.data);
                    var options = new JsonSerializerSettings() {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    status = JsonConvert.DeserializeObject<VoteStatus>(body, options);
                } catch (Exception e) {
                    Plugin.Log.Debug($"Exception: {e}");
                    failReason = $"Internal error: {e.Message}";
                    continue;
                }

                Plugin.Log.Debug("Vote success");
                LeaderboardState.VoteRequest.NotifyFinished(status);
                yield break; // if OK - stop retry cycle
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

        #region Utils

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
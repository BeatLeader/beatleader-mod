using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BeatLeader.Manager;
using BeatLeader.Models;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace BeatLeader.Utils {
    internal class HttpUtils {

        #region Get single entity

        internal IEnumerator GetData<T>(string url, Action<T> onSuccess, Action onFail) {
            var uri = new Uri(url);
            Plugin.Log.Debug($"Request url = {uri}");

            var handler = new DownloadHandlerBuffer();

            var request = new UnityWebRequest(url) {
                downloadHandler = handler
            };

            yield return request.SendWebRequest();
            Plugin.Log.Debug($"StatusCode: {request.responseCode}");

            if (request.isHttpError || request.isNetworkError) {
                onFail.Invoke();
                yield break;
            }

            try {
                var options = new JsonSerializerSettings() {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                var result = JsonConvert.DeserializeObject<T>(handler.text, options);
                onSuccess.Invoke(result);
            } catch (Exception) {
                onFail.Invoke();
            }
        }

        #endregion

        #region get list of entities

        internal IEnumerator GetPagedData<T>(string url, Action<Paged<T>> onSuccess, Action onFail) {
            var uri = new Uri(url);
            Plugin.Log.Debug($"Request url = {uri}");

            var handler = new DownloadHandlerBuffer();
            var request = new UnityWebRequest(url) {
                downloadHandler = handler
            };

            yield return request.SendWebRequest();
            Plugin.Log.Debug($"StatusCode: {request.responseCode}");

            if (request.isHttpError || request.isNetworkError) {
                onFail.Invoke();
                yield break;
            }

            try {
                var options = new JsonSerializerSettings() {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                var result = JsonConvert.DeserializeObject<Paged<T>>(handler.text, options);
                onSuccess.Invoke(result);
            } catch (Exception) {
                onFail.Invoke();
            }
        }

        #endregion

        #region ReplayUpload

        private static readonly int _retry = 3;

        public IEnumerator UploadReplay(Replay replay) {

            string authToken = BLContext.steamAuthToken;
            if (authToken == null) {
                Plugin.Log.Debug("No auth token, skip replay upload");
                yield break; // auth failed, no upload
            }

            LeaderboardEvents.NotifyUploadStarted();

            MemoryStream stream = new();
            ReplayEncoder.Encode(replay, new BinaryWriter(stream, Encoding.UTF8));

            for (int i = 1; i <= _retry; i++) {
                Plugin.Log.Debug($"Attempt to upload replay {i}/{_retry}");

                var request = new UnityWebRequest(BLConstants.REPLAY_UPLOAD_URL + "?ticket=" + authToken, UnityWebRequest.kHttpVerbPOST) {
                    downloadHandler = new DownloadHandlerBuffer(),
                    uploadHandler = new UploadHandlerRaw(stream.ToArray())
                };

                yield return request.SendWebRequest();

                Plugin.Log.Debug($"StatusCode: {request.responseCode}");

                var body = Encoding.UTF8.GetString(request.downloadHandler.data);
                if (body != null && body.Length > 0) {
                    if (!(body.StartsWith("{") || body.StartsWith("[") || body.StartsWith("<"))) {
                        Plugin.Log.Debug($"Response content: {body}");
                    }
                }

                try {
                    if (request.isNetworkError || request.isHttpError) {
                        Plugin.Log.Debug($"Error: {request.error}");
                        LeaderboardEvents.NotifyUploadFailed(i == _retry, i);
                    } else {
                        Plugin.Log.Debug(body);
                        var options = new JsonSerializerSettings() {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        Score score = JsonConvert.DeserializeObject<Score>(body, options);
                        // Plugin.Log.Debug(score.player.name); update profile from score.player ?
                        Plugin.Log.Debug("Upload success");

                        LeaderboardEvents.NotifyUploadSuccess();

                        yield break; // if OK - stop retry cycle
                    }
                } catch (Exception e) {
                    Plugin.Log.Debug("Exception");
                    Plugin.Log.Debug(e);
                    LeaderboardEvents.NotifyUploadFailed(i == _retry, i);
                }
            }
            Plugin.Log.Debug("Cannot upload replay");
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

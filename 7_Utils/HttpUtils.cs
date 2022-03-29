using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using BeatLeader.Models;
using System.Collections.Generic;
using System.Text;

namespace BeatLeader.Utils {
    internal class HttpUtils {
        private readonly HttpClient _client = new();

        #region get single entity

        internal async Task<T> GetData<T>(string url, CancellationToken token, T defaultValue) {
            var uri = new Uri(url);
            Plugin.Log.Debug($"Request url = {uri}");

            try {
                HttpResponseMessage response = await _client.GetAsync(uri, token);
                var body = await response.Content.ReadAsStringAsync();

                Plugin.Log.Debug($"StatusCode: {response.StatusCode}, ReasonPhrase: '{response.ReasonPhrase}'");

                if (response.IsSuccessStatusCode) {
                    var options = new JsonSerializerSettings() {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    return JsonConvert.DeserializeObject<T>(body, options);
                } else {
                    if (body != null && body.Length > 0) {
                        if (!(body.StartsWith("{") || body.StartsWith("[") || body.StartsWith("<"))) {
                            Plugin.Log.Debug($"Response content: {body}");
                        }
                    }
                }
            } catch (TaskCanceledException) {
                Plugin.Log.Debug("Request was cancelled");
            } catch (SocketException) {
                Plugin.Log.Error("Socket exception");
            } catch (Exception e) {
                Plugin.Log.Error("Exception");
                Plugin.Log.Error(e);
            }

            return defaultValue;
        }

        #endregion

        #region get list of entities

        internal async Task<Paged<T>> GetPagedData<T>(string url, CancellationToken token, Paged<T> defaultValue) {
            var uri = new Uri(url);
            Plugin.Log.Debug($"Request url = {uri}");

            try {
                HttpResponseMessage response = await _client.GetAsync(uri, token);
                var body = await response.Content.ReadAsStringAsync();

                Plugin.Log.Debug($"StatusCode: {response.StatusCode}, ReasonPhrase: '{response.ReasonPhrase}'");

                if (response.IsSuccessStatusCode) {
                    var options = new JsonSerializerSettings() {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    };
                    return JsonConvert.DeserializeObject<Paged<T>>(body, options);
                } else {
                    if (body != null && body.Length > 0) {
                        if (!(body.StartsWith("{") || body.StartsWith("[") || body.StartsWith("<"))) {
                            Plugin.Log.Debug($"Response content: {body}");
                        }
                    }
                }
            } catch (TaskCanceledException) {
                Plugin.Log.Debug("Request was cancelled");
            } catch (SocketException) {
                Plugin.Log.Error("Socket exception");
            } catch (Exception e) {
                Plugin.Log.Error("Exception");
                Plugin.Log.Error(e);
            }

            return defaultValue;
        }

        #endregion

        internal static string ToHttpParams(Dictionary<string, Object> param) {
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
    }
}

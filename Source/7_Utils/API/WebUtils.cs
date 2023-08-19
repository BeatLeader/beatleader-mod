using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Utils {
    internal static class WebUtils {
        public static readonly HttpClient HttpClient = new();

        public static async Task<byte[]?> SendRawDataRequestAsync(
            string url,
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            return await SendRawDataRequestAsync(new Uri(url), headersCallback);
        }

        public static async Task<byte[]?> SendRawDataRequestAsync(
            Uri uri,
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            return await SendAsync(uri, headersCallback: headersCallback) is { IsSuccessStatusCode: true } res
                ? await res.Content.ReadAsByteArrayAsync() : null;
        }

        public static async Task<T?> SendAndDeserializeAsync<T>(
            string url,
            string method = "GET",
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            return await SendAndDeserializeAsync<T>(new Uri(url), method, headersCallback);
        }

        public static async Task<T?> SendAndDeserializeAsync<T>(
            Uri uri,
            string method = "GET",
            Action<HttpRequestHeaders>? headersCallback = null
            ) {
            var res = await SendAsync(uri, method, headersCallback);
            if (res is not { IsSuccessStatusCode: true }) return default;
            try {
                return JsonConvert.DeserializeObject<T>(await res.Content.ReadAsStringAsync());
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to deserialize an object after request:\n" + ex);
                return default;
            }
        }

        public static async Task<HttpResponseMessage?> SendAsync(
            string url,
            string method = "GET",
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            return await SendAsync(new Uri(url, UriKind.Absolute), method, headersCallback);
        }

        public static async Task<HttpResponseMessage?> SendAsync(
            Uri uri,
            string method = "GET",
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            var request = new HttpRequestMessage {
                RequestUri = uri,
                Method = new(method)
            };
            headersCallback?.Invoke(request.Headers);
            try {
                return await HttpClient.SendAsync(request);
            } catch (Exception ex) {
                Plugin.Log.Error("Web request failed:\n" + ex);
                return null;
            }
        }
    }
}
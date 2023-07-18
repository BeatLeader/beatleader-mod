using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
            return await SendAsync(uri) is { IsSuccessStatusCode: true } res
                ? await res.Content.ReadAsByteArrayAsync() : null;
        }

        public static async Task<HttpResponseMessage> SendAsync(
            string url,
            string method = "GET",
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            return await SendAsync(new Uri(url, UriKind.Absolute), method, headersCallback);
        }

        public static async Task<HttpResponseMessage> SendAsync(
            Uri uri,
            string method = "GET",
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            var request = new HttpRequestMessage {
                RequestUri = uri,
                Method = new(method)
            };
            headersCallback?.Invoke(request.Headers);
            return await HttpClient.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> SendAsync(
            string url,
            IDictionary<string, string> headers,
            string method = "GET"
        ) {
            return await SendAsync(new Uri(url, UriKind.Absolute), headers, method);
        }

        public static async Task<HttpResponseMessage> SendAsync(
            Uri uri,
            IDictionary<string, string> headers,
            string method = "GET"
        ) {
            return await SendAsync(uri, method, x => {
                foreach (var item in headers) x.Add(item.Key, item.Value);
            });
        }
    }
}
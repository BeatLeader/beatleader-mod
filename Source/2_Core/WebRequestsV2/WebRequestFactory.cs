using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.WebRequests {
    public static class WebRequestFactory {
        private static readonly CookieContainer cookieContainer = new();
        private static readonly HttpClientHandler httpClientHandler = new() { CookieContainer = cookieContainer };
        private static readonly HttpClient httpClient = new(httpClientHandler);

        public static IWebRequest Send(
            HttpRequestMessage requestMessage,
            WebRequestParams? requestParams = null,
            CancellationToken token = default
        ) {
            ApplyDefaultHeaders(requestMessage);
            requestParams ??= new();
            return new WebRequestProcessor(SendInternal, requestMessage, requestParams, token);
        }

        public static IWebRequest<T> Send<T>(
            HttpRequestMessage requestMessage,
            IWebRequestResponseParser<T> responseParser,
            WebRequestParams? requestParams = null,
            CancellationToken token = default
        ) {
            ApplyDefaultHeaders(requestMessage);
            requestParams ??= new();
            return new WebRequestProcessor<T>(SendInternal, requestMessage, requestParams, responseParser, token);
        }

        private static Task<HttpResponseMessage?> SendInternal(
            HttpRequestMessage requestMessage,
            CancellationToken token
        ) {
            ApplyDefaultHeaders(requestMessage);
            return httpClient.SendAsync(requestMessage, token);
        }

        private static void ApplyDefaultHeaders(HttpRequestMessage requestMessage) {
            requestMessage.Headers.Add("User-Agent", $"PC mod {Plugin.Version} / {Application.version}");
        }
    }
}
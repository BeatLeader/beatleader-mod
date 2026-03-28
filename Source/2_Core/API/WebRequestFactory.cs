using System;
using BeatLeader.API;
using BeatLeader.Utils;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.WebRequests {
    public static class WebRequestFactory {
        internal static readonly CookieContainer CookieContainer = new();
        private static readonly HttpClientHandler httpClientHandler = new() { CookieContainer = CookieContainer};
        private static readonly HttpClient httpClient = new(httpClientHandler);

        static WebRequestFactory() {
            // ServicePointManager.DefaultConnectionLimit = 10;
            ServicePointManager.MaxServicePointIdleTime = 10_000;
            httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        }

        public static IWebRequest<object> Send(
                HttpRequestMessage requestMessage,
                WebRequestParams? requestParams = null,
                CancellationToken token = default,
                bool waitForLogin = true
            ) {
            requestParams ??= new();
            return new WebRequestProcessor<object>(waitForLogin ? SendInternalLogin : SendInternal, requestMessage, requestParams, null, token);
        }

        public static IWebRequest<T> Send<T>(
            HttpRequestMessage requestMessage,
            IWebRequestResponseParser<T> responseParser,
            WebRequestParams? requestParams = null,
            CancellationToken token = default,
            bool waitForLogin = true
        ) {
            requestParams ??= new();
            return new WebRequestProcessor<T>(waitForLogin ? SendInternalLogin : SendInternal, requestMessage, requestParams, responseParser, token);
        }

        private static Task<HttpResponseMessage?> SendInternal(
            HttpRequestMessage requestMessage,
            CancellationToken token
        ) {
            ApplyDefaultHeaders(requestMessage);
            return httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
        }

        private static Task<HttpResponseMessage> SendInternalLogin(
            HttpRequestMessage requestMessage,
            CancellationToken token
        ) {
            var sp = ServicePointManager.FindServicePoint(new Uri("https://api.beatleader.com/"));

            Plugin.Log.Warn($"CurrentConnections: {sp.CurrentConnections.ToString()}");
            Plugin.Log.Warn($"ConnectionLimit: {sp.ConnectionLimit.ToString()}");

            ApplyDefaultHeaders(requestMessage);
            return Task.Run(async () => {
                var loggedIn = await Authentication.WaitLogin();
                if (!loggedIn) return null;
                return await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
            }).RunCatching();
        }

        private static void ApplyDefaultHeaders(HttpRequestMessage requestMessage) {
            if (!requestMessage.Headers.Contains("User-Agent")) {
                requestMessage.Headers.Add("User-Agent", Plugin.UserAgent);
            }
            ServicePointManager.FindServicePoint(requestMessage.RequestUri).ConnectionLeaseTimeout = 30000;
        }
    }
}
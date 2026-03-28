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
            ServicePointManager.DefaultConnectionLimit = 20;
            ServicePointManager.MaxServicePointIdleTime = 10_000;
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

        private static Task<HttpResponseMessage?> SendInternalLogin(
            HttpRequestMessage requestMessage,
            CancellationToken token
        ) {
            ApplyDefaultHeaders(requestMessage);

            return Task.Run(async () => {
                var loggedIn = await Authentication.WaitLogin();
                if (!loggedIn) return null;
                return await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token);
            }).RunCatching();
        }

        private static void ApplyDefaultHeaders(HttpRequestMessage requestMessage) {
            requestMessage.Headers.Add("User-Agent", Plugin.UserAgent);
        }
    }
}
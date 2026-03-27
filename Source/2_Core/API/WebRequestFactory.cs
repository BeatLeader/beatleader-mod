using BeatLeader.API;
using BeatLeader.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.WebRequests {
    public static class WebRequestFactory {
        internal static readonly CookieContainer CookieContainer = new();
        private static readonly HttpClientHandler httpClientHandler = new() { CookieContainer = CookieContainer };
        private static readonly HttpClient httpClient = new(httpClientHandler);

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
            return httpClient.SendAsync(requestMessage, token);
        }

        private static Task<HttpResponseMessage?> SendInternalLogin(
            HttpRequestMessage requestMessage,
            CancellationToken token
        ) {
            ApplyDefaultHeaders(requestMessage);

            return Task.Run(async () => {
                await Authentication.WaitLogin();
                HttpResponseMessage? response = null;
                try {
                    response = await httpClient.SendAsync(requestMessage, token);
                } catch (Exception e) {
                    response?.Dispose();
                    response = null;
                }
                return response;
            }).RunCatching();
        }

        private static void ApplyDefaultHeaders(HttpRequestMessage requestMessage) {
            requestMessage.Headers.Add("User-Agent", Plugin.UserAgent);
        }
    }
}
using System;
using BeatLeader.APIV2;
using BeatLeader.Models;
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
                Func<HttpRequestMessage> requestMessageFactory,
                WebRequestParams? requestParams = null,
                CancellationToken token = default,
                bool waitForLogin = true
            ) {
            requestParams ??= new();
            PreSendRequestDelegate? preSendCallback = waitForLogin ? (requestToken) => WaitLogin(requestToken) : null;
            SendRequestDelegate sendCallback = (message, requestToken) => SendInternal(message, requestParams.ResponseCompletionOption, requestToken);

            return new WebRequestProcessor<object>(preSendCallback, sendCallback, requestMessageFactory, requestParams, null, token);
        }

        public static IWebRequest<T> Send<T>(
            Func<HttpRequestMessage> requestMessageFactory,
            IWebRequestResponseParser<T> responseParser,
            WebRequestParams? requestParams = null,
            CancellationToken token = default,
            bool waitForLogin = true
        ) {
            requestParams ??= new();
            PreSendRequestDelegate? preSendCallback = waitForLogin ? (requestToken) => WaitLogin(requestToken) : null;
            SendRequestDelegate sendCallback = (message, requestToken) => SendInternal(message, requestParams.ResponseCompletionOption, requestToken);

            return new WebRequestProcessor<T>(preSendCallback, sendCallback, requestMessageFactory, requestParams, responseParser, token);
        }

        private static Task<HttpResponseMessage?> SendInternal(
            HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption,
            CancellationToken token
        ) {
            ApplySelectedDomain(requestMessage);
            ApplyDefaultHeaders(requestMessage);
            return httpClient.SendAsync(requestMessage, completionOption, token);
        }

        private static Task<bool> WaitLogin(CancellationToken token) {
            return Task.Run(async () => {
                return await Authentication.WaitLogin();
            }).RunCatching();
        }

        private static void ApplyDefaultHeaders(HttpRequestMessage requestMessage) {
            requestMessage.Headers.Add("User-Agent", Plugin.UserAgent);
        }

        private static void ApplySelectedDomain(HttpRequestMessage requestMessage) {
            if (requestMessage.RequestUri == null) return;
            requestMessage.RequestUri = BeatLeaderServerUtils.ReplaceDomain(requestMessage.RequestUri);
        }
    }
}

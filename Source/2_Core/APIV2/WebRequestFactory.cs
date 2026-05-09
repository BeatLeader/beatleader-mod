using System;
using BeatLeader.APIV2;
using BeatLeader.Utils;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.WebRequests {
    public static class WebRequestFactory {
        internal static readonly CookieContainer CookieContainer = new();

        // The inner handler does NOT manage cookies — Ipv4PreferringHandler attaches and
        // captures them against the ORIGINAL hostname so auth keeps working after the URI
        // is rewritten to an IP literal.
        private static readonly HttpClientHandler httpClientHandler = new() {
            UseCookies = false,
            ServerCertificateCustomValidationCallback = Ipv4PreferringHandler.ValidateServerCertificate
        };

        private static readonly Ipv4PreferringHandler ipv4Handler = new(httpClientHandler, CookieContainer);
        private static readonly HttpClient httpClient = new(ipv4Handler);

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
            SendRequestDelegate sendCallback = waitForLogin
                ? (message, requestToken, startTimeout) => SendInternalLogin(message, requestParams.ResponseCompletionOption, requestToken, startTimeout)
                : (message, requestToken, startTimeout) => SendInternal(message, requestParams.ResponseCompletionOption, requestToken, startTimeout);

            return new WebRequestProcessor<object>(sendCallback, requestMessage, requestParams, null, token);
        }

        public static IWebRequest<T> Send<T>(
            HttpRequestMessage requestMessage,
            IWebRequestResponseParser<T> responseParser,
            WebRequestParams? requestParams = null,
            CancellationToken token = default,
            bool waitForLogin = true
        ) {
            requestParams ??= new();
            SendRequestDelegate sendCallback = waitForLogin
                ? (message, requestToken, startTimeout) => SendInternalLogin(message, requestParams.ResponseCompletionOption, requestToken, startTimeout)
                : (message, requestToken, startTimeout) => SendInternal(message, requestParams.ResponseCompletionOption, requestToken, startTimeout);

            return new WebRequestProcessor<T>(sendCallback, requestMessage, requestParams, responseParser, token);
        }

        private static Task<HttpResponseMessage?> SendInternal(
            HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption,
            CancellationToken token,
            Action startTimeout
        ) {
            ApplyDefaultHeaders(requestMessage);
            startTimeout();
            return httpClient.SendAsync(requestMessage, completionOption, token);
        }

        private static Task<HttpResponseMessage?> SendInternalLogin(
            HttpRequestMessage requestMessage,
            HttpCompletionOption completionOption,
            CancellationToken token,
            Action startTimeout
        ) {
            ApplyDefaultHeaders(requestMessage);

            return Task.Run(async () => {
                var loggedIn = await Authentication.WaitLogin();
                if (!loggedIn) return null;
                // Arm the per-request timeout only now that login is complete — otherwise queued
                // requests created long before login finishes are dead on arrival.
                startTimeout();
                return await httpClient.SendAsync(requestMessage, completionOption, token);
            }).RunCatching();
        }

        private static void ApplyDefaultHeaders(HttpRequestMessage requestMessage) {
            requestMessage.Headers.Add("User-Agent", Plugin.UserAgent);
        }
    }
}

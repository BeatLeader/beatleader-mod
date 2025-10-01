using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Utils;
using IPA.Utilities.Async;
using JetBrains.Annotations;

namespace BeatLeader.WebRequests {
    [PublicAPI]
    public abstract class PersistentWebRequestBase {
        protected static IWebRequest<object> Send(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            WebRequestParams? requestParams = null,
            Action<HttpRequestHeaders>? headersCallback = null,
            CancellationToken token = default,
            bool waitForLogin = true
        ) {
            var requestMessage = CreateAndValidateRequestMessage(url, method, content, headersCallback);
            return WebRequestFactory.Send(requestMessage, requestParams, token, waitForLogin);
        }

        protected static HttpRequestMessage CreateAndValidateRequestMessage(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            Action<HttpRequestHeaders>? headersCallback = null
        ) {
            var requestMessage = new HttpRequestMessage {
                RequestUri = new Uri(url),
                Method = method
            };
            if (content is not null) requestMessage.Content = content;
            headersCallback?.Invoke(requestMessage.Headers);
            return requestMessage;
        }
    }

    public abstract class PersistentWebRequestBase<TResult, TDescriptor> : PersistentWebRequestBase
        where TDescriptor : IWebRequestResponseParser<TResult>, new() {
        private static readonly TDescriptor descriptor = new();

        protected static IWebRequest<TResult> SendRet(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            WebRequestParams? requestParams = null,
            Action<HttpRequestHeaders>? headersCallback = null,
            CancellationToken token = default
        ) {
            var requestMessage = CreateAndValidateRequestMessage(url, method, content, headersCallback);
            return WebRequestFactory.Send(requestMessage, descriptor, requestParams, token);
        }
    }

    [PublicAPI]
    public abstract class PersistentSingletonWebRequestBase<T, TResult, TDescriptor> : PersistentWebRequestBase
        where T : PersistentSingletonWebRequestBase<T, TResult, TDescriptor>
        where TDescriptor : IWebRequestResponseParser<TResult>, new() {
        private static readonly TDescriptor descriptor = new();

        private static IWebRequest<TResult>? Instance = null;
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        protected static void SendRet(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            WebRequestParams? requestParams = null,
            Action<HttpRequestHeaders>? headersCallback = null,
            TDescriptor? customParser = default
        ) {
            var requestMessage = CreateAndValidateRequestMessage(url, method, content, headersCallback);

            if (Instance != null) {
                tokenSource.Cancel();
                tokenSource = new CancellationTokenSource();
                Instance.StateChangedEvent -= Instance_StateChangedEvent;
                Instance.ProgressChangedEvent -= Instance_ProgressChangedEvent;
            }

            Task.Run(async () => {
                    Instance = WebRequestFactory.Send(requestMessage, customParser ?? descriptor, requestParams, tokenSource.Token);
                    Instance.StateChangedEvent += Instance_StateChangedEvent;
                    Instance.ProgressChangedEvent += Instance_ProgressChangedEvent;
                    Instance_StateChangedEvent(Instance, RequestState, FailReason);
                }
            ).RunCatching();
        }

        private const string ObsoleteMessage = "This code was added for compatibility only. Rely on instances and async instead.";

        [Obsolete(ObsoleteMessage)]
        public static void Cancel() {
            if (Instance != null) {
                tokenSource.Cancel();
            }
        }

        [Obsolete(ObsoleteMessage)]
        public static TResult? Result => Instance != null ? Instance.Result : default;

        [Obsolete(ObsoleteMessage)]
        public static RequestState RequestState => Instance != null ? Instance.RequestState : RequestState.Uninitialized;

        [Obsolete(ObsoleteMessage)]
        public static HttpStatusCode RequestStatusCode => Instance != null ? Instance.RequestStatusCode : default;

        [Obsolete(ObsoleteMessage)]
        public static string? FailReason => Instance != null ? Instance.FailReason : default;

        [Obsolete(ObsoleteMessage)]
        public static float DownloadProgress => Instance != null ? Instance.DownloadProgress : 0;

        [Obsolete(ObsoleteMessage)]
        public static float UploadProgress => Instance != null ? Instance.UploadProgress : 0;

        [Obsolete(ObsoleteMessage)]
        public static float OverallProgress => Instance != null ? Instance.OverallProgress : 0;
        
        private static event WebRequestStateChangedDelegate<IWebRequest<TResult>>? StateChangedEventInternal;

        [Obsolete(ObsoleteMessage)]
        public static event WebRequestStateChangedDelegate<IWebRequest<TResult>>? StateChangedEvent {
            add {
                StateChangedEventInternal += value;
                value?.Invoke(Instance, Instance != null ? RequestState : RequestState.Uninitialized, Instance != null ? FailReason : null);
            }
            remove {
                StateChangedEventInternal -= value;
            }
        }

        internal static void Instance_StateChangedEvent(IWebRequest<TResult> instance, RequestState state, string? failReason) {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => {
                    StateChangedEventInternal?.Invoke(instance, state, failReason);
                }
            ).RunCatching();
        }

        private static event WebRequestProgressChangedDelegate<IWebRequest<TResult>>? ProgressChangedEventInternal;

        [Obsolete(ObsoleteMessage)]
        public static event WebRequestProgressChangedDelegate<IWebRequest<TResult>>? ProgressChangedEvent {
            add {
                ProgressChangedEventInternal += value;
                if (Instance != null) value?.Invoke(Instance, DownloadProgress, UploadProgress, OverallProgress);
            }
            remove {
                ProgressChangedEventInternal -= value;
            }
        }

        internal static void Instance_ProgressChangedEvent(IWebRequest<TResult> instance, float downloadProgress, float uploadProgress, float overallProgress) {
            UnityMainThreadTaskScheduler.Factory.StartNew(() => {
                    ProgressChangedEventInternal?.Invoke(instance, downloadProgress, uploadProgress, overallProgress);
                }
            ).RunCatching();
        }
    }
}
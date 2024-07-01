using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using JetBrains.Annotations;

namespace BeatLeader.WebRequests {
    [PublicAPI]
    public abstract class PersistentWebRequestBase<T, TRequest>
        where T : PersistentWebRequestBase<T, TRequest>
        where TRequest : IWebRequest {

        protected static IWebRequest Send(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            WebRequestParams? requestParams = null,
            Action<HttpRequestHeaders>? headersCallback = null,
            CancellationToken token = default
        ) {
            var requestMessage = CreateAndValidateRequestMessage(url, method, content, headersCallback);
            return WebRequestFactory.Send(requestMessage, requestParams, token);
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

    public abstract class PersistentWebRequestBaseWithResult<T, TResult, TDescriptor> : PersistentWebRequestBase<T, IWebRequest<TResult>>
        where T : PersistentWebRequestBaseWithResult<T, TResult, TDescriptor>
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
}
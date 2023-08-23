using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BeatLeader.WebRequests {
    [PublicAPI]
    public abstract class PersistentWebRequestBase<T, TRequest> : Singleton<T>
        where T : PersistentWebRequestBase<T, TRequest>
        where TRequest : IWebRequest {

        protected static IWebRequest Send(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            Action<HttpRequestHeaders>? headersCallback = null,
            int bufferSize = 4096,
            CancellationToken token = default
        ) {
            var requestMessage = CreateAndValidateRequestMessage(url, method, content, headersCallback);
            return WebRequestFactory.Send(requestMessage, new byte[bufferSize], token);
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

    public abstract class PersistentWebRequestWithResult<T, TResult, TDescriptor> : PersistentWebRequestBase<T, IWebRequest<TResult>>
        where T : PersistentWebRequestWithResult<T, TResult, TDescriptor>
        where TDescriptor : IWebRequestDescriptor<TResult>, new() {

        private static readonly TDescriptor descriptor = new();

        protected static IWebRequest<TResult> SendRet(
            string url,
            HttpMethod method,
            HttpContent? content = null,
            Action<HttpRequestHeaders>? headersCallback = null,
            int bufferSize = 4096,
            CancellationToken token = default
        ) {
            var requestMessage = CreateAndValidateRequestMessage(url, method, content, headersCallback);
            return WebRequestFactory.Send(requestMessage, descriptor, new byte[bufferSize], token);
        }
    }
}
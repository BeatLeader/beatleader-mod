using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader.WebRequests {
    public static class WebRequestFactory {
        private class IoOperationDescriptorAdapter : IIoOperationDescriptor {
            public IoOperationDescriptorAdapter(byte[] transferBuffer) {
                _buffer = transferBuffer;
            }

            #region OperationDescriptor

            byte[] IIoOperationDescriptor.Buffer => _buffer;

            void IIoOperationDescriptor.OnProgressChanged(long bytesRead, long totalBytes) {
                _originalDescriptor?.OnProgressChanged(bytesRead, totalBytes);
            }

            #endregion

            #region Init

            private IIoOperationDescriptor? _originalDescriptor;
            private readonly byte[] _buffer;

            public void Init(IIoOperationDescriptor originalDescriptor) {
                _originalDescriptor = originalDescriptor;
            }

            #endregion
        }

        private static readonly HttpClient httpClient = new();

        public static IWebRequest Send(
            HttpRequestMessage requestMessage,
            byte[] transferBuffer,
            CancellationToken token = default
        ) {
            var descriptorAdapter = ValidateHttpRequestContent(requestMessage, transferBuffer);
            var processor = SendInternal(requestMessage, x =>
                new WebRequestProcessor(transferBuffer, x, token), token);
            descriptorAdapter?.Init(processor);
            return processor;
        }

        public static IWebRequest<T> Send<T>(
            HttpRequestMessage requestMessage,
            IWebRequestDescriptor<T> descriptor,
            byte[] transferBuffer,
            CancellationToken token = default
        ) {
            var descriptorAdapter = ValidateHttpRequestContent(requestMessage, transferBuffer);
            var processor = SendInternal(requestMessage, x =>
                new WebRequestProcessor<T>(transferBuffer, descriptor, x, token), token);
            descriptorAdapter?.Init(processor);
            return (IWebRequest<T>)processor;
        }

        private static WebRequestProcessor SendInternal(
            HttpRequestMessage requestMessage,
            Func<Task<HttpResponseMessage?>, WebRequestProcessor> createProcessorCallback,
            CancellationToken token
        ) {
            var task = httpClient.SendAsync(requestMessage, token);
            var processor = createProcessorCallback(task);
            return processor;
        }

        private static IoOperationDescriptorAdapter? ValidateHttpRequestContent(HttpRequestMessage requestMessage, byte[] buffer) {
            if (requestMessage.Content is null) return null;
            var descriptorAdapter = new IoOperationDescriptorAdapter(buffer);
            requestMessage.Content = new HttpContentWithProgress(requestMessage.Content, descriptorAdapter);
            return descriptorAdapter;
        }
    }
}
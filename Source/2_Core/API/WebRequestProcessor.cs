using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.WebRequests {
    internal delegate Task<HttpResponseMessage?> SendRequestDelegate(HttpRequestMessage request, CancellationToken token);

    internal class WebRequestProcessor<T> : IWebRequest<T>, IIoOperationDescriptor, IDisposable {
        public WebRequestProcessor(
            SendRequestDelegate sendCallback,
            HttpRequestMessage requestMessage,
            WebRequestParams requestParams,
            IWebRequestResponseParser<T>? requestResponseParser,
            CancellationToken token
        ) {
            ValidateHttpMessage(requestMessage);
            _sendCallback = sendCallback;
            _requestMessage = requestMessage;
            RequestParams = requestParams;
            _requestTask = SendWebRequest(sendCallback, token);
            _processTask = ProcessWebRequest(token);
            _requestResponseParser = requestResponseParser;
            _cancellationToken = token;
        }

        public T? Result { get; private set; }

        private readonly IWebRequestResponseParser<T>? _requestResponseParser;

        protected async Task<RequestState?> ProcessResponse(
            HttpResponseMessage message,
            CancellationToken token
        ) {
            Headers = message.Content.Headers;
            var contentLength = message.Content.Headers.ContentLength;

            byte[] contentBuffer;
            if (contentLength == null || contentLength == 0) { 
                contentBuffer = new byte[] { };
            } else if (contentLength < 1000) {
                contentBuffer = await message.Content.ReadAsByteArrayAsync();
            } else {
                contentBuffer = await DownloadContent(message.Content, token);
            }
            if (token.IsCancellationRequested) return RequestState.Failed;
            try {
                if (_requestResponseParser != null) {
                    Result = _requestResponseParser.ParseResponse(contentBuffer);
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to parse web request response!\n{ex}");
            }
            return RequestState.Finished;
        }

        private async Task<byte[]> DownloadContent(
            HttpContent content,
            CancellationToken token
        ) {
            _treatReceivedProgressAsDownload = true;

            var descriptor = (IIoOperationDescriptor)this;
            descriptor.ContentSize = content.Headers.ContentLength!.Value;
            return await TransferContent(content, descriptor, token);
        }

        private static async Task<byte[]> TransferContent(
            HttpContent content,
            IIoOperationDescriptor descriptor,
            CancellationToken token
        ) {
            using var stream = await content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            await StreamUtils.CopyToByBufferAsync(stream, memoryStream, descriptor, token);
            return memoryStream.ToArray();
        }

        

        #region Validation

        private void ValidateHttpMessage(HttpRequestMessage requestMessage) {
            var content = requestMessage.Content;
            if (content is null) return;
            requestMessage.Content = new HttpContentWithProgress(content, this);
        }

        #endregion

        #region Request Progress

        public float DownloadProgress => _downloadProgress;
        public float OverallProgress => (UploadProgress + DownloadProgress) / 2;

        private bool _treatReceivedProgressAsDownload;
        private float _downloadProgress;

        protected void OnProgressChanged(float progress) {
            if (_treatReceivedProgressAsDownload) {
                _downloadProgress = progress;
            } else {
                UploadProgress = progress;
            }
        }

        protected long AdjustBufferSize(long size) {
            if (_treatReceivedProgressAsDownload) {
                return (long)(size / RequestParams.DownloadTrackingPrecision);
            } else {
                return (long)(size / RequestParams.UploadTrackingPrecision);;
            }
        }

        public float UploadProgress {
            get => _uploadProgress;
            private set {
                _uploadProgress = value;
                InvokeProgressEvent();
            }
        }

        private float _uploadProgress;

        #endregion

        #region Request State

        public RequestState RequestState {
            get => _requestState;
            protected set {
                _requestState = value;
                InvokeStateEvent();
            }
        }

        public HttpStatusCode RequestStatusCode { get; private set; }
        public string? FailReason { get; private set; }

        private RequestState _requestState = RequestState.Uninitialized;

        public async Task<IWebRequest<T>> Join() {
            await _processTask;
            return this;
        }

        public int RetryAttempt = 0;

        #endregion

        #region Request Events

        public event WebRequestStateChangedDelegate<IWebRequest<T>>? StateChangedEvent;
        public event WebRequestProgressChangedDelegate<IWebRequest<T>>? ProgressChangedEvent;

        private void InvokeStateEvent() {
            StateChangedEvent?.Invoke(this, RequestState, FailReason);
        }

        private void InvokeProgressEvent() {
            ProgressChangedEvent?.Invoke(this, DownloadProgress, UploadProgress, OverallProgress);
        }

        #endregion

        #region SendWebRequest

        private async Task<HttpResponseMessage?> SendWebRequest(SendRequestDelegate sendCallback, CancellationToken token) {
            var timeout = RequestParams.TimeoutSeconds;
            var timeoutTokenSource = GetTimeoutTokenSource(TimeSpan.FromSeconds(timeout), token);
            //
            try {
                return await sendCallback(_requestMessage, CancellationTokenSource.CreateLinkedTokenSource(token, timeoutTokenSource?.Token ?? CancellationToken.None).Token);
                //
            } catch (OperationCanceledException) when (!token.IsCancellationRequested) {
                //
                throw new TimeoutException($"The request has failed after {timeout}s");
            }
        }

        private static CancellationTokenSource? GetTimeoutTokenSource(TimeSpan timeout, CancellationToken cancellationToken) {
            if (timeout == Timeout.InfiniteTimeSpan) return null;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            return cts;
        }

        #endregion

        #region ProcessWebRequest

        protected WebRequestParams RequestParams { get; }

        private Task<HttpResponseMessage?> _requestTask;
        private readonly HttpRequestMessage _requestMessage;
        private readonly SendRequestDelegate _sendCallback;
        private readonly CancellationToken _cancellationToken;
        private readonly Task _processTask;

        private async Task ProcessWebRequest(CancellationToken token) {
            try {
                Plugin.Log.Info($"[Request({_requestTask.GetHashCode()})]: {_requestMessage.RequestUri}");

                RequestState = RequestState.Started;
                var result = await _requestTask;
                if (_requestTask.IsFaulted || result is null) {
                    if (RetryAttempt < RequestParams.RetryCount) {
                        await Retry();
                        return;
                    } else {
                        throw _requestTask.Exception ?? new Exception("Request task faulted for unknown reason");
                    }
                }

                UploadProgress = 1;
                RequestStatusCode = result.StatusCode;
                if (result.IsSuccessStatusCode) {
                    var newState = await ProcessResponse(result, token);

                    RequestState = newState ?? RequestState.Finished;

                    Plugin.Log.Info($"[Request({_requestTask.GetHashCode()})] Status code: {RequestStatusCode}");
                } else {
                    await ProcessFailure(result, null);
                }
            } catch (Exception ex) {
                await ProcessFailure(null, ex);
            }
            Dispose();
        }

        private async Task ProcessFailure(HttpResponseMessage? httpResponse, Exception? ex) {
            if (ex != null) {
                RequestState = RequestState.Failed;
                FailReason = "Exception occured, please report on Discord";
                Plugin.Log.Info($"[Request({_requestTask.GetHashCode()})] Exception: {ex}");
            } else if (httpResponse != null) {
                NetworkingUtils.GetRequestFailReason(httpResponse, out string failReason, out bool shouldRetry);

                if (shouldRetry && RetryAttempt < RequestParams.RetryCount) {
                    await Retry();
                    return;
                } else {
                    RequestState = RequestState.Failed;
                    FailReason = failReason;
                    Plugin.Log.Info($"[Request({_requestTask.GetHashCode()})] Fail reason: {failReason}");
                }
            }
        }

        private async Task Retry() {
            RetryAttempt++;

            _requestTask = SendWebRequest(_sendCallback, _cancellationToken);
            await ProcessWebRequest(_cancellationToken);
        }

        #endregion

        #region IO Buffer

        private static readonly List<byte[]> bufferPool = new();

        byte[] IIoOperationDescriptor.Buffer {
            get => _buffer ?? throw new InvalidOperationException("Unable to access buffer");
        }

        long IIoOperationDescriptor.ContentSize {
            set => ReloadBuffer(value);
        }

        public HttpContentHeaders? Headers { get; private set; }

        private byte[]? _buffer;

        private void ReloadBuffer(long size) {
            ReleaseBufferIfNeeded();
            size = AdjustBufferSize(size);
            var buffer = bufferPool.FirstOrDefault(x => x.LongLength >= size);
            if (buffer is not null) {
                bufferPool.Remove(buffer);
            } else {
                buffer = new byte[size];
            }
            _buffer = buffer;
        }

        private void ReleaseBufferIfNeeded() {
            if (_buffer is null) return;
            bufferPool.Add(_buffer);
            _buffer = null;
        }

        public void Dispose() => ReleaseBufferIfNeeded();

        #endregion

        #region IO Progress

        void IIoOperationDescriptor.OnProgressChanged(long bytesRead, long totalBytes) {
            var progress = bytesRead / (float)totalBytes;
            OnProgressChanged(progress);
        }

        #endregion
    }
}
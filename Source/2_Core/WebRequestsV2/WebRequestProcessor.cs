using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.WebRequests {
    internal delegate Task<HttpResponseMessage?> SendRequestDelegate(HttpRequestMessage request, CancellationToken token);

    internal class WebRequestProcessor<T> : WebRequestProcessor, IWebRequest<T> {
        public WebRequestProcessor(
            SendRequestDelegate sendCallback,
            HttpRequestMessage requestMessage,
            WebRequestParams requestParams,
            IWebRequestResponseParser<T> requestResponseParser,
            CancellationToken cancellationToken
        ) : base(
            sendCallback,
            requestMessage,
            requestParams,
            cancellationToken
        ) {
            _requestResponseParser = requestResponseParser;
        }

        #region WebRequest

        public T? Result { get; private set; }

        public new event WebRequestStateChangedDelegate<IWebRequest<T>>? StateChangedEvent {
            add => base.StateChangedEvent += (WebRequestStateChangedDelegate<IWebRequest>)value!;
            remove => base.StateChangedEvent -= (WebRequestStateChangedDelegate<IWebRequest>)value!;
        }

        public new event WebRequestProgressChangedDelegate<IWebRequest<T>>? ProgressChangedEvent {
            add => base.ProgressChangedEvent += (WebRequestProgressChangedDelegate<IWebRequest>)value!;
            remove => base.ProgressChangedEvent += (WebRequestProgressChangedDelegate<IWebRequest>)value!;
        }

        public new async Task<IWebRequest<T>> Join() {
            await base.Join();
            return this;
        }

        #endregion

        #region ProcessResponse

        private readonly IWebRequestResponseParser<T> _requestResponseParser;

        protected override async Task<RequestState?> ProcessResponse(
            HttpResponseMessage message,
            CancellationToken token
        ) {
            var contentLength = message.Content.Headers.ContentLength;
            if (contentLength is null or 0) return RequestState.Finished;

            var contentBuffer = await DownloadContent(message.Content, token);
            if (token.IsCancellationRequested) return RequestState.Cancelled;
            try {
                RequestState = RequestState.Parsing;
                Result = await _requestResponseParser.ParseResponse(contentBuffer);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to parse web request response!\n{ex}");
            }
            return null;
        }

        private async Task<byte[]> DownloadContent(
            HttpContent content,
            CancellationToken token
        ) {
            RequestState = RequestState.Downloading;
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

        #endregion

        #region Request Progress

        public override float DownloadProgress => _downloadProgress;
        public override float OverallProgress => (UploadProgress + DownloadProgress) / 2;

        private bool _treatReceivedProgressAsDownload;
        private float _downloadProgress;

        protected override void OnProgressChanged(float progress) {
            if (_treatReceivedProgressAsDownload) {
                _downloadProgress = progress;
            } else {
                base.OnProgressChanged(progress);
            }
        }

        protected override long AdjustBufferSize(long size) {
            if (_treatReceivedProgressAsDownload) {
                return (long)(size / RequestParams.DownloadTrackingPrecision);
            } else {
                return base.AdjustBufferSize(size);
            }
        }

        #endregion
    }

    internal class WebRequestProcessor : IWebRequest, IIoOperationDescriptor, IDisposable {
        public WebRequestProcessor(
            SendRequestDelegate sendCallback,
            HttpRequestMessage requestMessage,
            WebRequestParams requestParams,
            CancellationToken token
        ) {
            ValidateHttpMessage(requestMessage);
            _requestMessage = requestMessage;
            RequestParams = requestParams;
            _requestTask = sendCallback(requestMessage, token);
            _processTask = ProcessWebRequest(token);
        }

        #region Validation

        private void ValidateHttpMessage(HttpRequestMessage requestMessage) {
            var content = requestMessage.Content;
            if (content is null) return;
            requestMessage.Content = new HttpContentWithProgress(content, this);
        }

        #endregion

        #region Request Progress

        public float UploadProgress {
            get => _uploadProgress;
            private set {
                _uploadProgress = value;
                InvokeProgressEvent();
            }
        }

        public virtual float DownloadProgress => 0;
        public virtual float OverallProgress => UploadProgress;

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

        public async Task<IWebRequest> Join() {
            await _processTask;
            return this;
        }

        #endregion

        #region Request Events

        public event WebRequestStateChangedDelegate<IWebRequest>? StateChangedEvent;
        public event WebRequestProgressChangedDelegate<IWebRequest>? ProgressChangedEvent;

        private void InvokeStateEvent() {
            StateChangedEvent?.Invoke(this, RequestState, FailReason);
        }

        private void InvokeProgressEvent() {
            ProgressChangedEvent?.Invoke(this, DownloadProgress, UploadProgress, OverallProgress);
        }

        #endregion

        #region ProcessWebRequest

        protected WebRequestParams RequestParams { get; }

        private readonly Task<HttpResponseMessage?> _requestTask;
        private readonly HttpRequestMessage _requestMessage;
        private readonly Task _processTask;

        private async Task ProcessWebRequest(CancellationToken token) {
            try {
                Plugin.Log.Debug($"[Request({_requestTask.GetHashCode()})]: {_requestMessage.RequestUri}");

                RequestState = RequestState.Uploading;
                var result = await _requestTask;
                if (_requestTask.IsFaulted || result is null) {
                    throw _requestTask.Exception ?? new Exception("Request task faulted for unknown reason");
                }

                UploadProgress = 1;
                RequestStatusCode = result.StatusCode;
                if (!result.IsSuccessStatusCode) {
                    throw new WebException($"Unsuccessful request. Status code: {result.StatusCode}");
                }

                var newState = await ProcessResponse(result, token);
                RequestState = newState ?? RequestState.Finished;

                Plugin.Log.Debug($"[Request({_requestTask.GetHashCode()})] Status code: {RequestStatusCode}");
            } catch (Exception ex) {
                RequestState = RequestState.Failed;
                FailReason = ex.Message;
                Plugin.Log.Debug($"[Request({_requestTask.GetHashCode()})] Fail reason: {FailReason}");
            }
            Dispose();
        }

        protected virtual Task<RequestState?> ProcessResponse(
            HttpResponseMessage message,
            CancellationToken token
        ) {
            return Task.FromResult(default(RequestState?));
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

        protected virtual long AdjustBufferSize(long size) {
            return (long)(size / RequestParams.UploadTrackingPrecision);
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

        protected virtual void OnProgressChanged(float progress) {
            UploadProgress = progress;
        }

        #endregion
    }
}
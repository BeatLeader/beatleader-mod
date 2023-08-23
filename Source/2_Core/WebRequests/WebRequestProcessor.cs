using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.WebRequests {
    internal class WebRequestProcessor<T> : WebRequestProcessor, IWebRequest<T> {
        public WebRequestProcessor(
            byte[] transferBuffer,
            IWebRequestDescriptor<T> requestDescriptor,
            Task<HttpResponseMessage?> requestTask,
            CancellationToken cancellationToken
        ) : base(
            transferBuffer,
            requestTask,
            cancellationToken
        ) {
            _requestDescriptor = requestDescriptor;
        }

        #region WebRequest

        public T? Result { get; private set; }

        #endregion

        #region ProcessResponse

        private readonly IWebRequestDescriptor<T> _requestDescriptor;

        protected override async Task<RequestState?> ProcessResponse(HttpResponseMessage message) {
            if (message.Content.Headers.ContentLength is 0) return RequestState.Finished;
            //download result
            RequestState = RequestState.Downloading;
            using var stream = await message.Content.ReadAsStreamAsync();
            using var memoryStream = new MemoryStream();
            _treatReceivedProgressAsDownload = true;
            await StreamUtils.CopyToByBufferAsync(stream, memoryStream, this, _cancellationToken);

            if (_cancellationToken.IsCancellationRequested) return RequestState.Cancelled;
            //parse result
            RequestState = RequestState.Parsing;
            try {
                Result = await _requestDescriptor.ParseResponse(memoryStream.ToArray());
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to parse web request response!\n{ex}");
            }
            return null;
        }

        #endregion
    }

    internal class WebRequestProcessor : IWebRequest, IIoOperationDescriptor {
        public WebRequestProcessor(
            byte[] transferBuffer,
            Task<HttpResponseMessage?> requestTask,
            CancellationToken cancellationToken
        ) {
            _transferBuffer = transferBuffer;
            _requestTask = requestTask;
            _cancellationToken = cancellationToken;
            _processTask = ProcessWebRequest();
        }

        #region WebRequest

        public RequestState RequestState {
            get => _requestState;
            protected set {
                _requestState = value;
                InvokeStateEvent();
            }
        }

        public HttpStatusCode RequestStatusCode { get; private set; }
        public string? FailReason { get; private set; }

        public float DownloadProgress {
            get => _downloadProgress;
            private set {
                _downloadProgress = value;
                InvokeProgressEvent();
            }
        }

        public float UploadProgress {
            get => _uploadProgress;
            private set {
                _uploadProgress = value;
                InvokeProgressEvent();
            }
        }

        public virtual float OverallProgress => (UploadProgress + DownloadProgress) / 2;

        public async Task<IWebRequest> Join() {
            await _processTask;
            return this;
        }

        #endregion

        #region Events

        public event WebRequestStateChangedDelegate? StateChangedEvent;
        public event WebRequestProgressChangedDelegate? ProgressChangedEvent;

        private void InvokeStateEvent() {
            StateChangedEvent?.Invoke(this, RequestState, FailReason);
        }

        private void InvokeProgressEvent() {
            ProgressChangedEvent?.Invoke(this, DownloadProgress, UploadProgress, OverallProgress);
        }

        #endregion

        #region ProcessWebRequest

        private readonly Task _processTask;
        private readonly Task<HttpResponseMessage?> _requestTask;
        private readonly byte[] _transferBuffer;
        protected CancellationToken _cancellationToken;

        private RequestState _requestState = RequestState.Uninitialized;
        private float _uploadProgress;
        private float _downloadProgress;
        protected bool _treatReceivedProgressAsDownload;

        protected virtual Task<RequestState?> ProcessResponse(HttpResponseMessage message) => Task.FromResult(default(RequestState?));

        private async Task ProcessWebRequest() {
            try {
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
                var newState = await ProcessResponse(result);
                DownloadProgress = 1;
                RequestState = newState ?? RequestState.Finished;
            } catch (Exception ex) {
                RequestState = RequestState.Failed;
                FailReason = ex.Message;
            }
        }

        #endregion

        #region OperationDescriptor

        byte[] IIoOperationDescriptor.Buffer => _transferBuffer;

        void IIoOperationDescriptor.OnProgressChanged(long bytesRead, long totalBytes) {
            var progress = bytesRead / (float)totalBytes;
            if (_treatReceivedProgressAsDownload) DownloadProgress = progress;
            else UploadProgress = progress;
        }

        #endregion
    }
}
using System;

namespace BeatLeader.API.RequestHandlers {
    public class SimpleRequestHandler<T> : IWebRequestHandler<T> {
        private readonly Action<T> _onSuccess;
        private readonly Action<string> _onFail;

        public SimpleRequestHandler(Action<T> onSuccess, Action<string> onFail) {
            _onSuccess = onSuccess;
            _onFail = onFail;
        }

        public void OnRequestFinished(T result) {
            _onSuccess(result);
        }

        public void OnRequestFailed(string reason) {
            _onFail(reason);
        }

        public void OnRequestStarted() { }

        public void OnUploadProgress(float progress) { }

        public void OnDownloadProgress(float progress) { }

        public void OnProgress(float progress) { }
    }
}
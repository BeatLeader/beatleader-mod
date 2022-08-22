using System;
using UnityEngine;

namespace BeatLeader.API.RequestHandlers {
    internal abstract class PersistentSingletonRequestHandler<T, R> : PersistentSingleton<T>, IWebRequestHandler<R> where T : MonoBehaviour {
        #region State

        public delegate void StateChangedDelegate(RequestState state, R result, string failReason);

        public event StateChangedDelegate StateChangedEvent;

        private RequestState _requestState = RequestState.Uninitialized;
        public R Result { get; private set; }
        public string FailReason { get; private set; } = "";

        public RequestState State {
            get => _requestState;
            private set {
                _requestState = value;
                StateChangedEvent?.Invoke(value, Result, FailReason);
            }
        }

        public void AddStateListener(StateChangedDelegate handler) {
            StateChangedEvent += handler;
            handler?.Invoke(State, Result, FailReason);
        }

        public void RemoveStateListener(StateChangedDelegate handler) {
            StateChangedEvent -= handler;
        }

        #endregion

        #region Progress

        public event Action<float> DownloadProgressChangedEvent;
        public event Action<float> UploadProgressChangedEvent;
        public event Action<float> ProgressChangedEvent;

        private float _downloadProgress;

        public float DownloadProgress {
            get => _downloadProgress;
            private set {
                _downloadProgress = value;
                DownloadProgressChangedEvent?.Invoke(value);
            }
        }


        private float _uploadProgress;

        public float UploadProgress {
            get => _uploadProgress;
            private set {
                _uploadProgress = value;
                UploadProgressChangedEvent?.Invoke(value);
            }
        }


        private float _progress;

        public float Progress {
            get => _progress;
            private set {
                _progress = value;
                ProgressChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Send

        protected virtual bool AllowConcurrentRequests { get; } = false;

        private Coroutine _coroutine;

        protected void Send(IWebRequestDescriptor<R> requestDescriptor, int retries = 1) {
            if (!AllowConcurrentRequests && State is RequestState.Started && _coroutine != null) {
                StopCoroutine(_coroutine);
                OnRequestFailed("Cancelled");
            }

            _coroutine = StartCoroutine(NetworkingUtils.ProcessRequestCoroutine(requestDescriptor, this, retries));
        }

        #endregion

        #region Handler implementation

        public void OnRequestStarted() {
            Result = default;
            FailReason = "";
            State = RequestState.Started;
        }

        public void OnRequestFinished(R result) {
            Result = result;
            State = RequestState.Finished;
        }

        public void OnRequestFailed(string reason) {
            FailReason = reason;
            State = RequestState.Failed;
        }

        public void OnUploadProgress(float progress) {
            UploadProgress = progress;
        }

        public void OnDownloadProgress(float progress) {
            DownloadProgress = progress;
        }

        public void OnProgress(float progress) {
            Progress = progress;
        }

        #endregion
    }
}
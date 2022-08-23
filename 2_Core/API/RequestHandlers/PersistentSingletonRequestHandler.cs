using UnityEngine;

namespace BeatLeader.API.RequestHandlers {
    internal abstract class PersistentSingletonRequestHandler<T, R> : PersistentSingleton<PersistentSingletonRequestHandler<T, R>>, IWebRequestHandler<R> where T : MonoBehaviour {
        #region State

        public delegate void StateChangedDelegate(RequestState state, R result, string failReason);

        private event StateChangedDelegate StateChangedEvent;

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

        public static void AddStateListener(StateChangedDelegate handler) {
            if (!IsSingletonAvailable) return;
            instance.StateChangedEvent += handler;
            handler?.Invoke(instance.State, instance.Result, instance.FailReason);
        }

        public static void RemoveStateListener(StateChangedDelegate handler) {
            if (!IsSingletonAvailable) return;
            instance.StateChangedEvent -= handler;
        }

        #endregion

        #region Progress

        public delegate void ProgressChangedDelegate(float uploadProgress, float downloadProgress, float overallProgress);

        private event ProgressChangedDelegate ProgressChangedEvent;

        public float UploadProgress { get; private set; }
        public float DownloadProgress { get; private set; }
        public float OverallProgress { get; private set; }

        private void SetProgress(float uploadProgress, float downloadProgress, float overallProgress) {
            UploadProgress = uploadProgress;
            DownloadProgress = downloadProgress;
            OverallProgress = overallProgress;
            ProgressChangedEvent?.Invoke(uploadProgress, downloadProgress, overallProgress);
        }

        public static void AddProgressListener(ProgressChangedDelegate handler) {
            if (!IsSingletonAvailable) return;
            instance.ProgressChangedEvent += handler;
            handler?.Invoke(instance.UploadProgress, instance.DownloadProgress, instance.OverallProgress);
        }

        public static void RemoveProgressListener(ProgressChangedDelegate handler) {
            if (!IsSingletonAvailable) return;
            instance.ProgressChangedEvent -= handler;
        }

        #endregion

        #region Send

        protected virtual bool AllowConcurrentRequests { get; } = false;

        private Coroutine _coroutine;

        public void Send(IWebRequestDescriptor<R> requestDescriptor, int retries = 1) {
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

        public void OnProgress(float uploadProgress, float downloadProgress, float combinedProgress) {
            SetProgress(uploadProgress, downloadProgress, combinedProgress);
        }

        #endregion
    }
}
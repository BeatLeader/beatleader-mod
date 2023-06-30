using UnityEngine;

namespace BeatLeader.API.RequestHandlers {
    public abstract class PersistentSingletonRequestHandler<T, R> : MonoSingleton<T>, IWebRequestHandler<R> where T : MonoBehaviour {
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
            var tmp = Instance as PersistentSingletonRequestHandler<T, R>;
            tmp!.StateChangedEvent += handler;
            handler?.Invoke(tmp.State, tmp.Result, tmp.FailReason);
        }

        public static void RemoveStateListener(StateChangedDelegate handler) {
            var tmp = Instance as PersistentSingletonRequestHandler<T, R>;
            tmp!.StateChangedEvent -= handler;
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
            var tmp = Instance as PersistentSingletonRequestHandler<T, R>;
            tmp!.ProgressChangedEvent += handler;
            handler?.Invoke(tmp.UploadProgress, tmp.DownloadProgress, tmp.OverallProgress);
        }

        public static void RemoveProgressListener(ProgressChangedDelegate handler) {
            var tmp = Instance as PersistentSingletonRequestHandler<T, R>;
            tmp!.ProgressChangedEvent -= handler;
        }

        #endregion

        #region Properties
        
        protected virtual bool AllowConcurrentRequests { get; } = false;
        protected virtual bool KeepState { get; } = true;

        #endregion

        #region Send

        private Coroutine _coroutine;

        protected void Send(IWebRequestDescriptor<R> requestDescriptor, int retries = 1, int timeoutSeconds = 30) {
            if (!AllowConcurrentRequests && State is RequestState.Started && _coroutine != null) {
                StopCoroutine(_coroutine);
                OnRequestFailed("Cancelled");
            }

            _coroutine = StartCoroutine(NetworkingUtils.ProcessRequestCoroutine(requestDescriptor, this, retries, timeoutSeconds));
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
            ClearStateIfNeeded();
        }

        public void OnRequestFailed(string reason) {
            FailReason = reason;
            State = RequestState.Failed;
            ClearStateIfNeeded();
        }

        public void OnProgress(float uploadProgress, float downloadProgress, float combinedProgress) {
            SetProgress(uploadProgress, downloadProgress, combinedProgress);
        }

        private void ClearStateIfNeeded() {
            if (KeepState) return;
            Result = default;
            FailReason = "";
            State = RequestState.Uninitialized;
        }

        #endregion
    }
}
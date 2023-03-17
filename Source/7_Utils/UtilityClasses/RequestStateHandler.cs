using System;

namespace BeatLeader {
    internal class RequestStateHandler<T> {
        #region Events

        public delegate void StateChangedDelegate(RequestState state);

        public delegate void FinishedDelegate(T result);

        public delegate void FailedDelegate(string reason);

        public event StateChangedDelegate StateChangedEvent;
        public event FinishedDelegate FinishedEvent;
        public event FailedDelegate FailedEvent;
        public event Action StartedEvent;

        #endregion

        #region State

        public T Result;
        public string FailReason;

        private RequestState _state = RequestState.Uninitialized;

        public RequestState State {
            get => _state;
            private set {
                _state = value;
                StateChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Notify

        public void NotifyStarted() {
            State = RequestState.Started;
            StartedEvent?.Invoke();
        }

        public void NotifyFailed(string reason) {
            Result = default;
            FailReason = reason;
            State = RequestState.Failed;
            FailedEvent?.Invoke(reason);
        }

        public void NotifyFinished(T result) {
            Result = result;
            FailReason = "";
            State = RequestState.Finished;
            FinishedEvent?.Invoke(result);
        }

        public void TryNotifyCancelled() {
            if (State != RequestState.Started) return;
            NotifyFailed("Cancelled");
        }

        #endregion
    }

    public enum RequestState {
        Uninitialized,
        Started,
        Failed,
        Finished
    }
}
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
            set {
                if (_state.Equals(value)) return;
                _state = value;
                StateChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Notify

        public void NotifyStarted() {
            StartedEvent?.Invoke();
            State = RequestState.Started;
        }

        public void NotifyFailed(string reason) {
            Result = default;
            FailReason = reason;
            FailedEvent?.Invoke(reason);
            State = RequestState.Failed;
        }

        public void NotifyFinished(T result) {
            Result = result;
            FailReason = "";
            FinishedEvent?.Invoke(result);
            State = RequestState.Finished;
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
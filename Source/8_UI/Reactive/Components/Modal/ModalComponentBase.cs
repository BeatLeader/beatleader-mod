using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface IModal {
        bool OffClickCloses { get; }

        event Action? ModalAskedToBeClosedEvent;

        void Pause();
        void Resume();
        void Close();
        void OnOpen();
    }

    internal abstract class ModalComponentBase : ReactiveComponent, IModal {
        #region Abstraction

        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnClose() { }
        protected virtual void OnOpen() { }

        #endregion

        #region IModal Impl

        public virtual bool OffClickCloses => true;
        public virtual bool AllowExternalClose => true;

        public event Action? ModalAskedToBeClosedEvent;

        public void Pause() {
            OnPause();
        }

        public void Resume() {
            OnResume();
        }

        public void Close() {
            if (!AllowExternalClose) return;
            CloseInternal();
        }

        void IModal.OnOpen() {
            OnOpen();
        }

        protected void CloseInternal() {
            ModalAskedToBeClosedEvent?.Invoke();
            OnClose();
        }

        protected override void OnInitialize() {
            Enabled = false;
        }

        #endregion
    }
}
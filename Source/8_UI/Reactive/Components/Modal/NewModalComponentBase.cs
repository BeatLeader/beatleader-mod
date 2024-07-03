using System;

namespace BeatLeader.UI.Reactive.Components {
    internal interface INewModal : IAnimationProgressProvider {
        event Action<INewModal, bool>? ModalClosedEvent;
        event Action<INewModal, bool>? ModalOpenedEvent;
        event Action<INewModal, float>? OpenProgressChangedEvent;

        void Pause();
        void Resume();
        void Close(bool immediate);
        void Open(bool immediate);
    }

    //TODO: rename to ModalComponentBase
    internal abstract class NewModalComponentBase : ReactiveComponent, INewModal {
        #region Abstraction

        protected virtual bool AllowExternalClose => true;

        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnClose(bool closed) { }
        protected virtual void OnOpen(bool opened) { }

        #endregion

        #region Modal
        
        protected bool IsOpened { get; private set; }
        protected bool IsPaused { get; private set; }
        
        public event Action<INewModal, bool>? ModalClosedEvent;
        public event Action<INewModal, bool>? ModalOpenedEvent;
        public event Action<INewModal, float>? OpenProgressChangedEvent;
        
        public void Pause() {
            if (IsPaused) return;
            IsPaused = true;
            OnPause();
        }

        public void Resume() {
            if (!IsPaused) return;
            IsPaused = false;
            OnResume();
        }

        public void Close(bool immediate) {
            if (!AllowExternalClose) return;
            CloseInternal(immediate);
        }

        public void Open(bool immediate) {
            if (IsOpened) return;
            IsOpened = true;
            //in case we are opening when modal is in closing process
            if (_needToInvokeCloseEvent) {
                OnClose(true);
                _needToInvokeCloseEvent = false;
            }
            OnOpen(false);
            _needToInvokeOpenEvent = true;
            Enabled = true;
            RefreshOpenAnimation(true, immediate);
            ModalOpenedEvent?.Invoke(this, false);
        }

        protected void CloseInternal(bool immediate = false) {
            if (!IsOpened) return;
            IsOpened = false;
            //in case we are closing when modal is in opening process
            if (_needToInvokeOpenEvent) {
                OnOpen(true);
                _needToInvokeOpenEvent = false;
            }
            OnClose(false);
            _needToInvokeCloseEvent = true;
            RefreshOpenAnimation(false, immediate);
            ModalClosedEvent?.Invoke(this, false);
        }

        #endregion

        #region Open Animation

        float IAnimationProgressProvider.AnimationProgress => OpenProgress;

        protected virtual float OpenProgress => _openValueAnimator.Progress;

        private readonly TimeValueAnimator _openValueAnimator = new();
        private bool _needToInvokeOpenEvent;
        private bool _needToInvokeCloseEvent;

        protected virtual void RefreshOpenAnimation(bool goingToOpen, bool immediate) {
            _openValueAnimator.Duration = goingToOpen ? 0.2f : 0.07f;
            _openValueAnimator.Execute(!goingToOpen);
            if (!immediate) return;
            _openValueAnimator.SetProgress(goingToOpen ? 1f : 0f);
        }

        protected virtual void OnOpenAnimationProgressChanged(float progress) { }

        private void UpdateOpenAnimation() {
            _openValueAnimator.Update();
            OpenProgressChangedEvent?.Invoke(this, _openValueAnimator.Progress);
            OnOpenAnimationProgressChanged(_openValueAnimator.Progress);
            //disabling when finished
            if (!IsOpened && _openValueAnimator.Progress == 0f) {
                Enabled = false;
            }
        }

        #endregion

        #region Update

        protected virtual void OnUpdateInternal() { }

        protected sealed override void OnUpdate() {
            OnUpdateInternal();
            UpdateOpenAnimation();
            //checking for events invocation
            if (_needToInvokeOpenEvent && 1f - OpenProgress < 0.001f) {
                OnOpen(true);
                _needToInvokeOpenEvent = false;
                ModalOpenedEvent?.Invoke(this, true);
            } else if (_needToInvokeCloseEvent && OpenProgress < 0.001f) {
                OnClose(true);
                _needToInvokeCloseEvent = false;
                ModalClosedEvent?.Invoke(this, true);
            }
        }

        #endregion
    }
}
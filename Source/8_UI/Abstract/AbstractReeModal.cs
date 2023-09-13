using System;
using HMUI;

namespace BeatLeader {
    internal abstract class AbstractReeModal<TC> : ReeUIComponentV2, IReeModal {
        #region Context

        protected TC Context;

        public void ClearContext() {
            Context = default;
        }

        public void SetContext(TC context) {
            Context = context;
            OnContextChanged();
        }

        #endregion

        #region Protected

        protected bool offClickCloses = true;

        protected override void OnInitialize() {
            var bg = Content.GetComponent<ImageView>();
            if (bg != null) bg.raycastTarget = true;
        }

        protected virtual void OnContextChanged() { }
        protected virtual void OnResume() { }
        protected virtual void OnPause() { }
        protected virtual void OnInterrupt() { }

        protected virtual void OnOffClick() {
            if (offClickCloses) Close();
        }

        #endregion

        #region IReeModalImpl

        private Action _closeAction;

        public void Resume(object state, Action closeAction) {
            SetContext((TC)state);
            _closeAction = closeAction;
            gameObject.SetActive(true);
            Content.gameObject.SetActive(true);
            OnResume();
        }

        public void Pause() {
            gameObject.SetActive(false);
            Content.gameObject.SetActive(false);
            OnPause();
        }

        public void Interrupt() {
            gameObject.SetActive(false);
            Content.gameObject.SetActive(false);
            OnInterrupt();
        }

        public void Close() {
            _closeAction?.Invoke();
        }

        public void HandleOffClick() {
            OnOffClick();
        }

        #endregion
    }
}
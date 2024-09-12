using System;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class AnimatedModalWrapper<T> : AnimatedModalComponentBase where T : IReactiveComponent, new() {
        #region UI Props

        public T Component { get; } = new();
        public Action<T>? PauseCallback { get; set; }
        public Action<T>? ResumeCallback { get; set; }
        public Action<T, bool>? OpenCallback { get; set; }
        public Action<T, bool>? CloseCallback { get; set; }
        public bool ClickOffCloses { get; set; } = true;

        #endregion

        #region Modal

        protected override bool AllowExternalClose => ClickOffCloses;

        protected override void OnClose(bool closed) {
            CloseCallback?.Invoke(Component, closed);
        }

        protected override void OnOpen(bool opened) {
            OpenCallback?.Invoke(Component, opened);
        }

        protected override void OnPause() {
            PauseCallback?.Invoke(Component);
        }

        protected override void OnResume() {
            ResumeCallback?.Invoke(Component);
        }

        protected override GameObject Construct() {
            return Component.Use(null);
        }

        #endregion
    }
}
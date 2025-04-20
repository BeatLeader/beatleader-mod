using System;
using Reactive;

namespace BeatLeader.UI.Reactive.Components {
    internal class Dialog<T> : DialogBase where T : IReactiveComponent, ILayoutItem, new() {
        #region UI Props

        public new string Title {
            get => base.Title;
            set => base.Title = value;
        }

        public new bool ShowCancelButton {
            get => base.ShowCancelButton;
            set => base.ShowCancelButton = value;
        }

        public new bool CancelButtonInteractable {
            get => base.CancelButtonInteractable;
            set => base.CancelButtonInteractable = value;
        }

        public new bool OkButtonInteractable {
            get => base.OkButtonInteractable;
            set => base.OkButtonInteractable = value;
        }

        public Action<Dialog<T>>? OkButtonClickedCallback { get; set; }

        public Action<Dialog<T>>? CancelButtonClickedCallback { get; set; }

        #endregion

        #region Construct

        public T Component { get; } = new();

        protected override ILayoutItem ConstructContent() => Component;

        #endregion

        #region Callbacks

        protected override void OnOkButtonClicked() {
            if (OkButtonClickedCallback == null) {
                base.OnOkButtonClicked();
            } else {
                OkButtonClickedCallback(this);
            }
        }
        
        protected override void OnCancelButtonClicked() {
            if (CancelButtonClickedCallback == null) {
                base.OnOkButtonClicked();
            } else {
                CancelButtonClickedCallback(this);
            }
        }

        #endregion
    }
}
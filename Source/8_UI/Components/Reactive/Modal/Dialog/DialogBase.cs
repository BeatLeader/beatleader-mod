using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class DialogBase : ModalBase {
        #region UI Props

        protected string Title {
            get => _header.Text;
            set => _header.Text = value;
        }

        protected string CancelButtonText {
            get => _cancelButton.Text;
            set => _cancelButton.Text = value;
        }

        protected string OkButtonText {
            get => _okButtonLabel.Text;
            set => _okButtonLabel.Text = value;
        }

        protected bool ShowCancelButton {
            get => _cancelButton.Enabled;
            set => _cancelButton.Enabled = value;
        }

        protected bool CancelButtonInteractable {
            get => _cancelButton.Interactable;
            set => _cancelButton.Interactable = value;
        }

        protected bool OkButtonInteractable {
            get => _okButton.Interactable;
            set => _okButton.Interactable = value;
        }

        protected bool ShowOkButton {
            get => _okButton.Enabled;
            set => _okButton.Enabled = value;
        }

        #endregion

        #region Construct

        protected override bool AllowExternalClose => false;

        private DialogHeader _header = null!;
        private BsButton _cancelButton = null!;
        private BsPrimaryButton _okButton = null!;
        private Label _okButtonLabel = null!;

        protected sealed override GameObject Construct() {
            return new Background {
                Children = {
                    new DialogHeader()
                        .AsFlexItem(basis: 6f)
                        .Bind(ref _header),
                    //content
                    ConstructContent().AsFlexItem(flexGrow: 1f),
                    //
                    new Layout {
                        Children = {
                            new BsButton {
                                Text = "Cancel",
                                Skew = 0f,
                                OnClick = OnCancelButtonClicked
                            }.AsFlexItem(flexGrow: 1f).Bind(ref _cancelButton),
                            //
                            new BsPrimaryButton {
                                    Text = "OK",
                                    Skew = 0f,
                                    OnClick = OnOkButtonClicked
                                }
                                .AsFlexItem(flexGrow: 1f)
                                .Bind(ref _okButton)
                                .Bind(ref _okButtonLabel)
                        }
                    }.AsFlexItem(basis: 8f).AsFlexGroup(padding: 1f, gap: 1f)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).AsBlurBackground().Use();
        }

        protected abstract ILayoutItem ConstructContent();

        #endregion

        #region Events

        protected virtual void OnOkButtonClicked() {
            CloseInternal();
        }

        protected virtual void OnCancelButtonClicked() {
            CloseInternal();
        }

        #endregion
    }
}
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class DialogComponentBase : AnimatedModalComponentBase {
        #region UI Props

        protected string Title {
            get => _header.Text;
            set => _header.Text = value;
        }

        protected string CancelButtonText {
            get => _cancelButtonLabel.Text;
            set => _cancelButtonLabel.Text = value;
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
        private ButtonBase _cancelButton = null!;
        private Label _cancelButtonLabel = null!;
        private ButtonBase _okButton = null!;
        private Label _okButtonLabel = null!;

        protected sealed override GameObject Construct() {
            return new Image {
                Children = {
                    new DialogHeader()
                        .AsFlexItem(basis: 6f)
                        .Bind(ref _header),
                    //content
                    ConstructContent().AsFlexItem(grow: 1f),
                    //
                    new Dummy {
                        Children = {
                            new BsButton {
                                    Skew = 0f
                                }
                                .WithLabel(out _cancelButtonLabel, "Cancel")
                                .WithClickListener(OnCancelButtonClicked)
                                .AsFlexItem(grow: 1f)
                                .Bind(ref _cancelButton),
                            //
                            new BsPrimaryButton {
                                    Skew = 0f
                                }
                                .WithLabel(out _okButtonLabel, "Ok")
                                .WithClickListener(OnOkButtonClicked)
                                .AsFlexItem(grow: 1f)
                                .Bind(ref _okButton)
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
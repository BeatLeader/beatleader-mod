using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal abstract class DialogComponentBase : ModalComponentBase {
        #region UI Props

        protected string Title {
            get => _header.Text;
            set => _header.Text = value;
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

        #endregion

        #region Construct

        public override bool OffClickCloses => false;
        public override bool AllowExternalClose => false;

        private DialogHeader _header = null!;
        private ButtonBase _cancelButton = null!;
        private ButtonBase _okButton = null!;

        protected sealed override GameObject Construct() {
            return new Image {
                Children = {
                    new DialogHeader()
                        .AsFlexItem(basis: 6f)
                        .Bind(ref _header),
                    //content
                    ConstructContent(),
                    //
                    new Dummy {
                        Children = {
                            new BsButton()
                                .WithLabel("Cancel")
                                .WithClickListener(OnCancelButtonClicked)
                                .AsFlexItem(grow: 1f)
                                .Bind(ref _cancelButton),
                            //
                            new BsPrimaryButton()
                                .WithLabel("Ok")
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
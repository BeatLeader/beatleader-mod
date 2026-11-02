using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class TagCreationDialog : Dialog {
        #region Construct

        private TagEditorPanel _tagEditorPanel = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            Title = "Create Tag";

            Children.Add(
                new TagEditorPanel {
                    FlexItem = {
                        FlexGrow = 1f,
                        Size = new() { x = 42.pt, y = 8.pt },
                        Margin = new() { top = 2f, left = 2f, right = 2f, bottom = 1f }
                    }
                }.Bind(ref _tagEditorPanel)
            );

            _tagEditorPanel.Validity.AddCallback(this, static (x, y) => x.OkButton.Interactable = y);

            OkButton.OnClick = HandleOkButtonClicked;
            CancelButton.OnClick = () => IsPushed = false;
            OnPushed += HandlePushed;
        }

        #endregion

        #region Callbacks

        private void HandlePushed() {
            _tagEditorPanel.Clear();
        }

        private void HandleOkButtonClicked() {
            ReplayMetadataManager.CreateTag(_tagEditorPanel.TagName, _tagEditorPanel.TagColor);
            IsPushed = false;
        }

        #endregion
    }
}
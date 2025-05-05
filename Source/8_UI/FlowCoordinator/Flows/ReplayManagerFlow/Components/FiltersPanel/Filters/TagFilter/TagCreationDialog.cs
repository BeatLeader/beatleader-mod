using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class TagCreationDialog : DialogBase {
        #region Construct

        private TagEditorPanel _tagEditorPanel = null!;

        protected override ILayoutItem ConstructContent() {
            return new TagEditorPanel()
                .WithListener(
                    x => x.IsValid,
                    x => OkButtonInteractable = x
                )
                .AsFlexItem(flexGrow: 1f, margin: 2f)
                .Bind(ref _tagEditorPanel);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.AsFlexItem(size: new() { x = 46.pt(), y = 24.pt() });
            Title = "Create Tag";
            _tagEditorPanel.ValidationContract = ReplayMetadataManager.ValidateTagName;
        }

        #endregion

        #region Callbacks

        protected override void OnOpen(bool finished) {
            if (finished) return;
            _tagEditorPanel.Clear();
        }

        protected override void OnOkButtonClicked() {
            ReplayMetadataManager.CreateTag(_tagEditorPanel.TagName, _tagEditorPanel.TagColor);
            CloseInternal();
        }

        #endregion
    }
}
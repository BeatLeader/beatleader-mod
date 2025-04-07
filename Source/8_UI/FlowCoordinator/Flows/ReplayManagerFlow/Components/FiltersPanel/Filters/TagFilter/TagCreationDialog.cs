using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;

namespace BeatLeader.UI.Hub {
    internal class TagCreationDialog : DialogComponentBase {
        #region Construct

        private TagEditorPanel _tagEditorPanel = null!;

        protected override ILayoutItem ConstructContent() {
            return new TagEditorPanel()
                .WithListener(
                    x => x.IsValid,
                    x => OkButtonInteractable = x
                )
                .AsFlexItem(grow: 1f, margin: 2f)
                .Bind(ref _tagEditorPanel);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WithSizeDelta(46f, 24f);
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
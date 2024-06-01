using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class TagCreationDialog : DialogComponentBase {
        #region Setup

        private IReplayTagManager? _tagManager;
        
        public void Setup(IReplayTagManager? tagManager) {
            _tagManager = tagManager;
            if (tagManager != null) {
                _tagEditorPanel.ValidationContract = tagManager.ValidateTag;
            }
        }

        #endregion

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
            this.WithRectSize(38f, 40f);
            Title = "Create Tag";
        }

        #endregion

        #region Callbacks

        protected override void OnOpen() {
            _tagEditorPanel.Clear();
        }

        protected override void OnOkButtonClicked() {
            if (_tagManager == null) return;
            var tag = _tagManager.CreateTag(_tagEditorPanel.TagName);
            tag.Color = _tagEditorPanel.TagColor;
            CloseInternal();
        }

        #endregion
    }
}
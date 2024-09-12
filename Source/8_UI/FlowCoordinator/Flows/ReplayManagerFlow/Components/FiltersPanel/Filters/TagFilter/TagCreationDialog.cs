using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;

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
            this.WithSizeDelta(46f, 24f);
            Title = "Create Tag";
        }

        #endregion

        #region Callbacks

        protected override void OnOpen(bool finished) {
            if (finished) return;
            _tagEditorPanel.Clear();
        }

        protected override void OnOkButtonClicked() {
            if (_tagManager == null) return;
            var tag = _tagManager.CreateTag(_tagEditorPanel.TagName);
            tag.SetColor( _tagEditorPanel.TagColor);
            CloseInternal();
        }

        #endregion
    }
}
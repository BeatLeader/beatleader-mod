using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class TagDeletionDialog : DialogComponentBase {
        #region Setup

        private IReplayTagManager? _tagManager;
        private IEditableReplayTag? _replayTag;
        
        public void SetTag(IEditableReplayTag tag) {
            _replayTag = tag;
            _label.Text = $"Do you really want to delete {tag.Name}?";
        }

        public void Setup(IReplayTagManager replayManager) {
            _tagManager = replayManager;
        }
        
        #endregion

        #region Construct

        private Label _label = null!;

        protected override ILayoutItem ConstructContent() {
            return new Label {
                EnableWrapping = true
            }.Bind(ref _label).AsFlexItem(grow: 1f);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WithSizeDelta(60f, 26f);
            Title = "Delete Tag";
        }

        #endregion

        #region Callbacks

        protected override void OnOkButtonClicked() {
            base.OnOkButtonClicked();
            if (_tagManager == null || _replayTag == null) return;
            _tagManager.DeleteTag(_replayTag);
            _replayTag = null;
        }

        #endregion
    }
}
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class TagDeletionDialog : DialogBase {
        #region Setup

        private ReplayTag? _replayTag;

        public void SetTag(ReplayTag tag) {
            _replayTag = tag;
            _label.Text = $"Do you really want to delete {tag.Name}?";
        }

        #endregion

        #region Construct

        private Label _label = null!;

        protected override ILayoutItem ConstructContent() {
            return new Label {
                EnableWrapping = true
            }.Bind(ref _label).AsFlexItem(flexGrow: 1f);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.AsFlexItem(size: new() { x = 60.pt(), y = 26.pt() });
            Title = "Delete Tag";
        }

        #endregion

        #region Callbacks

        protected override void OnOkButtonClicked() {
            base.OnOkButtonClicked();
            if (_replayTag == null) {
                return;
            }

            ReplayMetadataManager.DeleteTag(_replayTag.Name);
            _replayTag = null;
        }

        #endregion
    }
}
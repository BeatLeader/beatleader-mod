using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class TagDeletionDialog : Dialog {
        #region Setup

        private ReplayTag? _replayTag;

        public void SetTag(ReplayTag tag) {
            _replayTag = tag;
            _label.Text = $"Do you really want to delete {tag.Name}?";
        }

        #endregion

        #region Construct

        private Label _label = null!;

        protected override void OnInitialize() {
            base.OnInitialize();
            Title = "Delete Tag";

            Children.Add(
                new Label {
                    FlexItem = {
                        FlexGrow = 1f,
                        Size = new() { x = 60.pt, y = 12.pt }
                    },

                    EnableWrapping = true
                }.Bind(ref _label)
            );

            OkButton.OnClick = HandleOkButtonClicked;
            CancelButton.OnClick = () => IsPushed = false;
        }

        #endregion

        #region Callbacks

        private void HandleOkButtonClicked() {
            IsPushed = false;
            if (_replayTag == null) {
                return;
            }

            ReplayMetadataManager.DeleteTag(_replayTag.Name);
            _replayTag = null;
        }

        #endregion
    }
}
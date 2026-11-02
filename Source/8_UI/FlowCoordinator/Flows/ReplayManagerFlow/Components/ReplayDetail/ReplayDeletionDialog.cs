using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class ReplayDeletionDialog : Dialog {
        #region Setup

        private IReplayHeader? _header;

        public void SetHeader(IReplayHeader header) {
            _header = header;
        }

        #endregion

        #region Construct

        protected override void OnInitialize() {
            base.OnInitialize();
            Title = "Delete Replay";

            Children.Add(
                new Label {
                    FlexItem = {
                        FlexGrow = 1f,
                        Size = new() { x = 60.pt, y = 14.pt }
                    },

                    Text = "Do you really want to delete this replay?\nYou won't be able to recover it."
                }
            );

            OkButton.OnClick = HandleOkButtonClicked;
            CancelButton.OnClick = () => IsPushed = false;
        }

        private void HandleOkButtonClicked() {
            IsPushed = false;

            if (_header != null) {
                ReplayManager.DeleteReplay(_header);
                _header = null;
            }
        }

        #endregion
    }
}
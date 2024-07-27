using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplayDeletionDialog : DialogComponentBase {
        #region Setup

        private IReplayHeader? _header;

        public void SetHeader(IReplayHeader header) {
            _header = header;
        }

        #endregion

        #region Construct

        protected override ILayoutItem ConstructContent() {
            return new Label {
                Text = "Do you really want to delete this replay?\nYou won't be able to recover it."
            };
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            this.WithSizeDelta(60f, 28f);
            Content.GetOrAddComponent<CanvasGroup>().ignoreParentGroups = true;
            Title = "Delete Replay";
        }

        protected override void OnOkButtonClicked() {
            base.OnOkButtonClicked();
            _header?.DeleteReplay();
            _header = null;
        }

        #endregion
    }
}
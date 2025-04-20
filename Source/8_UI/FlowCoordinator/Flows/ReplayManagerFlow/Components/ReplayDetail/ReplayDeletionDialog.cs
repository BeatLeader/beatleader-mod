using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;
using UnityExtensions = BeatLeader.Utils.UnityExtensions;

namespace BeatLeader.UI.Hub {
    internal class ReplayDeletionDialog : DialogBase {
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
            
            if (_header != null) {
                ReplayManager.DeleteReplay(_header);
                _header = null;
            }
        }

        #endregion
    }
}
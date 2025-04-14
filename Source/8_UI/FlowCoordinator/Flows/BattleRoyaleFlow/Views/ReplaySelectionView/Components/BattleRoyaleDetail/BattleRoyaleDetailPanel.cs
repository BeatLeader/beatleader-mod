using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleDetailPanel : BasicReplayDetailPanel {
        #region Construct

        private BsPrimaryButton _selectButton = null!;

        protected override ILayoutItem ConstructButtons() {
            return new BsPrimaryButton {
                    Text = BLLocalization.GetTranslation("ls-watch-replay-short"),
                    OnClick = HandleSelectButtonClicked
                }
                .AsFlexItem(size: new() { y = 8f })
                .Bind(ref _selectButton);
        }

        #endregion

        #region DetailPanel

        protected override bool AllowTagsEdit => false;
        protected override string EmptyText => "Select a replay to stop Monke from being a thief";

        private IBeatmapReplayLaunchPanel? _beatmapReplayLaunchPanel;

        public override void Setup(IBeatmapReplayLaunchPanel? launchPanel) {
            if (_beatmapReplayLaunchPanel != null) {
                _beatmapReplayLaunchPanel.SelectedReplaysUpdatedEvent -= HandleSelectedReplaysUpdated;
            }
            _beatmapReplayLaunchPanel = launchPanel;
            if (_beatmapReplayLaunchPanel != null) {
                _beatmapReplayLaunchPanel.SelectedReplaysUpdatedEvent += HandleSelectedReplaysUpdated;
            }
            UpdateSelectButton();
        }

        protected override Task SetDataInternalAsync(IReplayHeader header, CancellationToken token) {
            UpdateSelectButton();
            return Task.CompletedTask;
        }

        #endregion

        #region SelectButton

        private bool _replayIsAdded;

        private void UpdateSelectButton() {
            var interactable = Header != null && _beatmapReplayLaunchPanel != null;
            var containsCurrent = interactable && _beatmapReplayLaunchPanel!.SelectedReplays.Contains(Header);
            _selectButton.Interactable = interactable;
            _selectButton.Text = containsCurrent ? "Remove" : "Select";
            _replayIsAdded = containsCurrent;
        }

        #endregion

        #region Callbacks

        private void HandleSelectedReplaysUpdated() {
            UpdateSelectButton();
        }

        private void HandleSelectButtonClicked() {
            if (_replayIsAdded) {
                _beatmapReplayLaunchPanel!.RemoveSelectedReplay(Header!);
            } else {
                _beatmapReplayLaunchPanel!.AddSelectedReplay(Header!);
            }
            UpdateSelectButton();
        }

        #endregion
    }
}
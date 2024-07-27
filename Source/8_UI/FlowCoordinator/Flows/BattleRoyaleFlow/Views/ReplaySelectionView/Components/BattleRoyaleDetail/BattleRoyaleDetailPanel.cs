using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleDetailPanel : BasicReplayDetailPanel {
        #region Construct

        private ButtonBase _selectButton = null!;
        private Label _selectButtonLabel = null!;

        protected override ILayoutItem ConstructButtons() {
            return new BsPrimaryButton()
                .WithClickListener(HandleSelectButtonClicked)
                .WithLocalizedLabel(out _selectButtonLabel, "ls-watch-replay-short")
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
            _selectButtonLabel.Text = containsCurrent ? "Remove" : "Select";
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
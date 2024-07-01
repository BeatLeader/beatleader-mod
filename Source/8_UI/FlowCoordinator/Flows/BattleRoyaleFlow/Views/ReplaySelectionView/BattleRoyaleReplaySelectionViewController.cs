using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.UI.Hub {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.Flows.BattleRoyaleFlow.Views.ReplaysView.BattleRoyaleReplaysView.bsml")]
    internal class BattleRoyaleReplaySelectionViewController : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly IReplaysLoader _replaysLoader = null!;
        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region UI Components

        [UIComponent("replay-launch-panel"), UsedImplicitly]
        private BeatmapReplayLaunchPanel _replayLaunchPanel = null!;

        #endregion

        #region Setup

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            var detailPanel = BattleRoyaleDetailPanel.Instantiate(transform);
            _replayLaunchPanel.Setup(_replaysLoader);
            _replayLaunchPanel.DetailPanel = detailPanel;
            _replayLaunchPanel.ReplayFilter = _battleRoyaleHost.ReplayFilter;
            _replayLaunchPanel.ReplaySelectedEvent += HandleReplaySelected;
            _replayLaunchPanel.ReplayDeselectedEvent += HandleReplayDeselected;

            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;

            _replaysLoader.StartReplaysLoad();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
        }

        #endregion

        #region Callbacks

        private void HandleReplayAdded(IReplayHeaderBase header, object caller) {
            if (caller.Equals(this)) return;
            _replayLaunchPanel.AddSelectedReplay(header, false);
        }

        private void HandleReplayRemoved(IReplayHeaderBase header, object caller) {
            if (caller.Equals(this)) return;
            _replayLaunchPanel.RemoveSelectedReplay(header, false);
        }

        private void HandleReplaySelected(IReplayHeaderBase header) {
            _battleRoyaleHost.AddReplay(header, this);
        }

        private void HandleReplayDeselected(IReplayHeaderBase header) {
            _battleRoyaleHost.RemoveReplay(header, this);
        }

        #endregion
    }
}
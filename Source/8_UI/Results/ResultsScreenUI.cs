using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using JetBrains.Annotations;
using BeatLeader.Replayer;
using BeatLeader.Utils;

namespace BeatLeader.ViewControllers {
    internal class ResultsScreenUI : ReeUIComponentV2 {
        #region Setup

        [UIValue("voting-button"), UsedImplicitly]
        private readonly VotingButton _votingButton = InstantiateOnSceneRoot<VotingButton>(false);

        [UIValue("voting-panel"), UsedImplicitly]
        private readonly VotingPanel _votingPanel = InstantiateOnSceneRoot<VotingPanel>(false);

        [UIValue("replay-button")]
        private readonly ReplayButton _replayButton = InstantiateOnSceneRoot<ReplayButton>(false);

        private void Awake() {
            _votingButton.SetParent(transform);
            _votingPanel.SetParent(transform);
            _votingPanel.isOnResultsScreen = true;
            _replayButton.SetParent(transform);
            _replayButton.ReplayButtonClickedEvent += HandleReplayButtonClicked;
        }
        
        protected override void OnDispose() {
            _replayButton.ReplayButtonClickedEvent -= HandleReplayButtonClicked;
        }

        public void Refresh() {
            _replayButton.Interactable = ReplayManager.Instance.CachedReplay is not null;
        }

        #endregion

        #region Callbacks

        private void HandleReplayButtonClicked() {
            _ = ReplayerMenuLoader.Instance!.StartLastReplayAsync();
        }

        #endregion
    }
}

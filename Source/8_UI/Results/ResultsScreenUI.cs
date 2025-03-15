using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using JetBrains.Annotations;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatLeader.Manager;

namespace BeatLeader.ViewControllers {
    internal class ResultsScreenUI : ReeUIComponentV2 {
        #region Setup

        [UIValue("voting-button"), UsedImplicitly]
        private VotingButton _votingButton;

        [UIValue("replay-button")]
        private ReplayButton _replayButton;

        private void Awake() {
            _votingButton = ReeUIComponentV2.Instantiate<VotingButton>(transform, false);
            _replayButton = ReeUIComponentV2.Instantiate<ReplayButton>(transform, false);
            _replayButton.ReplayButtonClickedEvent += HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent += PresentVotingModal;
        }
        
        protected override void OnDestroy() {
            _replayButton.ReplayButtonClickedEvent -= HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent -= PresentVotingModal;
        }

        public void Refresh() {
            _replayButton.Interactable = ReplayManager.Instance.CachedReplay is not null;
        }

        #endregion

        #region Callbacks

        private void HandleReplayButtonClicked() {
            _ = ReplayerMenuLoader.Instance!.StartLastReplayAsync();
        }

        private void PresentVotingModal() {
            ReeModalSystem.OpenModal<VotingPanel>(transform, 0);
        }

        #endregion
    }
}

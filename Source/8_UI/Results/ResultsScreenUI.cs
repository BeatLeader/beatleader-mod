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
        private readonly VotingButton _votingButton = InstantiateOnSceneRoot<VotingButton>(false);

        [UIValue("replay-button")]
        private readonly ReplayButton _replayButton = InstantiateOnSceneRoot<ReplayButton>(false);

        private void Awake() {
            _votingButton.SetParent(transform);
            _replayButton.SetParent(transform);
            _replayButton.ReplayButtonClickedEvent += HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent += PresentVotingModal;
        }
        
        protected override void OnDestroy() {
            _replayButton.ReplayButtonClickedEvent -= HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent -= PresentVotingModal;
        }

        public void Refresh() {
            //TODO: inject
            //_replayButton.Interactable = ReplayManager.Instance.CachedReplay is not null;
        }

        #endregion

        #region Callbacks

        private void HandleReplayButtonClicked() {
            ReplayerMenuLoader.Instance!.StartLastReplayAsync().RunCatching();
        }

        private void PresentVotingModal() {
            ReeModalSystem.OpenModal<VotingPanel>(transform, 0);
        }

        #endregion
    }
}

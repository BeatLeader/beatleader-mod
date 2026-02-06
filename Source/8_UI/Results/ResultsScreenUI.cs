using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatLeader.Components;
using JetBrains.Annotations;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Results;
using BeatLeader.UI.MainMenu;
using Reactive.BeatSaber;
using UnityEngine;
using Zenject;

namespace BeatLeader.ViewControllers {
    internal class ResultsScreenUI : ReeUIComponentV2 {
        #region Setup

        [UIValue("voting-button"), UsedImplicitly]
        private VotingButton _votingButton;

        [UIValue("replay-button")]
        private ReplayButton _replayButton;

        private SpecialEventResultsPanel _eventResultsPanel = null!;
        private BeatLeaderNewsViewController? _newsViewController = null;

        private void Awake() {
            _newsViewController = Resources.FindObjectsOfTypeAll<BeatLeaderNewsViewController>().FirstOrDefault();
            if (_newsViewController != null) {
                _newsViewController.HappeningEvent.ValueChangedEvent += HandleEventStatusUpdated;
            }

            _eventResultsPanel = new();
            _eventResultsPanel.Use(transform);
            _eventResultsPanel.ContentTransform.localPosition = new(0f, 80f);

            _votingButton = ReeUIComponentV2.Instantiate<VotingButton>(transform, false);
            _replayButton = ReeUIComponentV2.Instantiate<ReplayButton>(transform, false);
            _replayButton.ReplayButtonClickedEvent += HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent += PresentVotingModal;
        }

        protected override void OnDestroy() {
            _replayButton.ReplayButtonClickedEvent -= HandleReplayButtonClicked;
            LeaderboardEvents.VotingWasPressedEvent -= PresentVotingModal;
        }

        public void Setup(BeatmapLevel level) {
            _replayButton.Interactable = ReplayManager.LastPlayedReplay is not null;

            if (_newsViewController != null) {
                var evt = _newsViewController.HappeningEvent.Value;
                var hash = CustomLevelLoader.kCustomLevelPrefixId + evt?.today?.song.hash.ToUpper();

                if (level.levelID == hash) {
                    _newsViewController.RefreshEventsCache();
                    _eventResultsPanel.Enabled = true;
                } else {
                    _eventResultsPanel.Enabled = false;
                }
            } else {
                _eventResultsPanel.Enabled = false;
            }
        }

        #endregion

        #region Callbacks

        private void HandleEventStatusUpdated(PlatformEventStatus? status) {
            if (status != null) {
                _eventResultsPanel.SetEvent(status);
                _eventResultsPanel.Enabled = true;
            } else {
                _eventResultsPanel.Enabled = false;
            }
        }

        private void HandleReplayButtonClicked() {
            ReplayerMenuLoader.Instance!.StartLastReplayAsync().RunCatching();
        }

        private void PresentVotingModal() {
            ReeModalSystem.OpenModal<VotingPanel>(transform, 0);
        }

        #endregion
    }
}
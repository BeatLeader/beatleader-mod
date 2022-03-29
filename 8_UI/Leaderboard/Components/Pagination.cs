using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.Pagination.bsml")]
    internal class Pagination : ReeUIComponent {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.ScoresRequestStartedEvent += OnScoreRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
        }

        protected override void OnDispose() {
            LeaderboardEvents.ScoresRequestStartedEvent -= OnScoreRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
        }

        #endregion

        #region Events

        private void OnScoreRequestStarted() {
            //Disable arrows to prevent multiple requests
        }

        private void OnScoresFetched(Paged<List<Score>> scoresData) {
            //Show arrows if there is more scores
        }

        #endregion

        #region Callbacks

        [UIAction("up-on-click"), UsedImplicitly]
        private void NavUpOnClick() {
            LeaderboardEvents.NotifyUpButtonWasPressed();
        }

        [UIAction("around-on-click"), UsedImplicitly]
        private void NavAroundOnClick() {
            LeaderboardEvents.NotifyAroundButtonWasPressed();
        }
        
        [UIAction("down-on-click"), UsedImplicitly]
        private void NavDownOnClick() {
            LeaderboardEvents.NotifyDownButtonWasPressed();
        }

        #endregion
    }
}
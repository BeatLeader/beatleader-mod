using BeatLeader.Components;
using BeatLeader.Manager;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController {
        #region Components

        [UIValue("scores-table"), UsedImplicitly]
        private ScoresTable _scoresTable = ReeUIComponent.Instantiate<ScoresTable>();

        [UIValue("pagination"), UsedImplicitly]
        private Pagination _pagination = ReeUIComponent.Instantiate<Pagination>();

        [UIValue("scope-selector"), UsedImplicitly]
        private ScopeSelector _scopeSelector = ReeUIComponent.Instantiate<ScopeSelector>();

        #endregion

        #region OnEnable & OnDisable

        protected void OnEnable() {
            LeaderboardEvents.NotifyIsLeaderboardVisibleChanged(true);
        }

        protected void OnDisable() {
            LeaderboardEvents.NotifyIsLeaderboardVisibleChanged(false);
        }

        #endregion
    }
}
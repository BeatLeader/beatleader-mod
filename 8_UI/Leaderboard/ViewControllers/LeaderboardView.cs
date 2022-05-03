using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController {
        #region Components

        [UIValue("leaderboard-settings"), UsedImplicitly]
        private LeaderboardSettings _leaderboardSettings = ReeUIComponentV2.InstantiateOnSceneRoot<LeaderboardSettings>(false);

        [UIValue("score-info-panel"), UsedImplicitly]
        private ScoreInfoPanel _scoreInfoPanel = ReeUIComponentV2.InstantiateOnSceneRoot<ScoreInfoPanel>(false);

        [UIValue("scores-table"), UsedImplicitly]
        private ScoresTable _scoresTable = ReeUIComponentV2.InstantiateOnSceneRoot<ScoresTable>();

        [UIValue("pagination"), UsedImplicitly]
        private Pagination _pagination = ReeUIComponentV2.InstantiateOnSceneRoot<Pagination>(false);

        [UIValue("scope-selector"), UsedImplicitly]
        private ScopeSelector _scopeSelector = ReeUIComponentV2.InstantiateOnSceneRoot<ScopeSelector>(false);

        [UIValue("context-selector"), UsedImplicitly]
        private ContextSelector _contextSelector = ReeUIComponentV2.InstantiateOnSceneRoot<ContextSelector>(false);

        [UIValue("empty-board-message"), UsedImplicitly]
        private EmptyBoardMessage _emptyBoardMessage = ReeUIComponentV2.InstantiateOnSceneRoot<EmptyBoardMessage>();

        private void Awake() {
            _leaderboardSettings.SetParent(transform);
            _scoreInfoPanel.SetParent(transform);
            _scoresTable.SetParent(transform);
            _pagination.SetParent(transform);
            _scopeSelector.SetParent(transform);
            _contextSelector.SetParent(transform);
            _emptyBoardMessage.SetParent(transform);
        }

        #endregion

        #region OnEnable & OnDisable

        protected void OnEnable() {
            LeaderboardState.IsVisible = true;
        }

        protected void OnDisable() {
            LeaderboardState.IsVisible = false;
        }

        #endregion
    }
}
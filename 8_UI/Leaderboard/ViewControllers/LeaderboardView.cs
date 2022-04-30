using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController {
        #region Components

        [UIValue("score-info-panel"), UsedImplicitly]
        private ScoreInfoPanel _scoreInfoPanel;

        [UIValue("scores-table"), UsedImplicitly]
        private ScoresTable _scoresTable = ReeUIComponent.Instantiate<ScoresTable>(false);

        [UIValue("pagination"), UsedImplicitly]
        private Pagination _pagination;

        [UIValue("scope-selector"), UsedImplicitly]
        private ScopeSelector _scopeSelector;

        [UIValue("context-selector"), UsedImplicitly]
        private ContextSelector _contextSelector;

        [UIValue("empty-board-message"), UsedImplicitly]
        private EmptyBoardMessage _emptyBoardMessage;

        private void Awake() {
            _scoreInfoPanel = ReeUIComponentV2.Instantiate<ScoreInfoPanel>(transform);
            _pagination = ReeUIComponentV2.Instantiate<Pagination>(transform);
            _scopeSelector = ReeUIComponentV2.Instantiate<ScopeSelector>(transform);
            _contextSelector = ReeUIComponentV2.Instantiate<ContextSelector>(transform);
            _emptyBoardMessage = ReeUIComponentV2.Instantiate<EmptyBoardMessage>(transform);
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
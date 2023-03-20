using BeatLeader.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController {
        #region PreParser

        [Inject, UsedImplicitly]
        private PreParser _preParser;

        public class PreParser : MonoBehaviour {
            public VotingPanel votingPanel;
            public BeatLeaderInfo beatLeaderInfo;
            public LeaderboardSettings leaderboardSettings;
            public ScoreInfoPanel scoreInfoPanel;
            public ScoresTable scoresTable;
            public VotingButton votingButton;
            public Pagination pagination;
            public ScopeSelector scopeSelector;
            public ContextSelector contextSelector;
            public EmptyBoardMessage emptyBoardMessage;

            private void Awake() {
                votingPanel = ReeUIComponentV2.InstantiateOnSceneRoot<VotingPanel>(false);
                beatLeaderInfo = ReeUIComponentV2.InstantiateOnSceneRoot<BeatLeaderInfo>(false);
                leaderboardSettings = ReeUIComponentV2.InstantiateOnSceneRoot<LeaderboardSettings>(false);
                scoreInfoPanel = ReeUIComponentV2.InstantiateOnSceneRoot<ScoreInfoPanel>(false);
                scoresTable = ReeUIComponentV2.InstantiateOnSceneRoot<ScoresTable>();
                votingButton = ReeUIComponentV2.InstantiateOnSceneRoot<VotingButton>(false);
                pagination = ReeUIComponentV2.InstantiateOnSceneRoot<Pagination>(false);
                scopeSelector = ReeUIComponentV2.InstantiateOnSceneRoot<ScopeSelector>(false);
                contextSelector = ReeUIComponentV2.InstantiateOnSceneRoot<ContextSelector>(false);
                emptyBoardMessage = ReeUIComponentV2.InstantiateOnSceneRoot<EmptyBoardMessage>();
            }
        }

        #endregion

        #region Components

        [UIValue("voting-panel"), UsedImplicitly]
        private VotingPanel VotingPanel => _preParser.votingPanel;

        [UIValue("beat-leader-info"), UsedImplicitly]
        private BeatLeaderInfo BeatLeaderInfo => _preParser.beatLeaderInfo;

        [UIValue("leaderboard-settings"), UsedImplicitly]
        private LeaderboardSettings LeaderboardSettings => _preParser.leaderboardSettings;

        [UIValue("score-info-panel"), UsedImplicitly]
        private ScoreInfoPanel ScoreInfoPanel => _preParser.scoreInfoPanel;

        [UIValue("scores-table"), UsedImplicitly]
        private ScoresTable ScoresTable => _preParser.scoresTable;

        [UIValue("voting-button"), UsedImplicitly]
        private VotingButton VotingButton => _preParser.votingButton;

        [UIValue("pagination"), UsedImplicitly]
        private Pagination Pagination => _preParser.pagination;

        [UIValue("scope-selector"), UsedImplicitly]
        private ScopeSelector ScopeSelector => _preParser.scopeSelector;

        [UIValue("context-selector"), UsedImplicitly]
        private ContextSelector ContextSelector => _preParser.contextSelector;

        [UIValue("empty-board-message"), UsedImplicitly]
        private EmptyBoardMessage EmptyBoardMessage => _preParser.emptyBoardMessage;

        private void Awake() {
            VotingPanel.SetParent(transform);
            BeatLeaderInfo.SetParent(transform);
            LeaderboardSettings.SetParent(transform);
            ScoreInfoPanel.SetParent(transform);
            ScoresTable.SetParent(transform);
            VotingButton.SetParent(transform);
            Pagination.SetParent(transform);
            ScopeSelector.SetParent(transform);
            ContextSelector.SetParent(transform);
            EmptyBoardMessage.SetParent(transform);
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
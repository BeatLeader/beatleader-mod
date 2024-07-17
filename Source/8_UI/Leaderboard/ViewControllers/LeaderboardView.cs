using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Leaderboard.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly IReplayerViewNavigator _replayerNavigator = null!;
        [Inject] private readonly SoloFreePlayFlowCoordinator _soloFlowCoordinator = null!;

        #endregion

        #region PreParser

        [Inject, UsedImplicitly]
        private PreParser _preParser;

        public class PreParser : MonoBehaviour {
            public ScoresTable scoresTable;
            public VotingButton votingButton;
            public Pagination pagination;
            public ScopeSelector scopeSelector;
            public ContextSelector contextSelector;
            public EmptyBoardMessage emptyBoardMessage;
            public MapDifficultyPanel mapDifficultyPanel;

            private void Awake() {
                scoresTable = ReeUIComponentV2.InstantiateOnSceneRoot<ScoresTable>();
                votingButton = ReeUIComponentV2.InstantiateOnSceneRoot<VotingButton>(false);
                pagination = ReeUIComponentV2.InstantiateOnSceneRoot<Pagination>(false);
                scopeSelector = ReeUIComponentV2.InstantiateOnSceneRoot<ScopeSelector>(false);
                contextSelector = ReeUIComponentV2.InstantiateOnSceneRoot<ContextSelector>(false);
                emptyBoardMessage = ReeUIComponentV2.InstantiateOnSceneRoot<EmptyBoardMessage>();
                mapDifficultyPanel = ReeUIComponentV2.InstantiateOnSceneRoot<MapDifficultyPanel>();
            }
        }

        #endregion

        #region Components

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

        [UIValue("map-difficulty-panel"), UsedImplicitly]
        private MapDifficultyPanel MapDifficultyPanel => _preParser.mapDifficultyPanel;

        private IReplayerStarter _replayerStarter = null!;

        private void Awake() {
            ScoresTable.SetParent(transform);
            VotingButton.SetParent(transform);
            Pagination.SetParent(transform);
            ScopeSelector.SetParent(transform);
            ContextSelector.SetParent(transform);
            EmptyBoardMessage.SetParent(transform);
            MapDifficultyPanel.SetParent(transform);
            _replayerStarter = new ReplayerNavigatingStarter(_soloFlowCoordinator, true, _replayerNavigator);
        }

        #endregion

        #region OnEnable & OnDisable

        protected void OnEnable() {
            LeaderboardEvents.ScoreInfoButtonWasPressed += PresentScoreInfoModal;
            LeaderboardEvents.LeaderboardSettingsButtonWasPressedEvent += PresentSettingsModal;
            LeaderboardEvents.LogoWasPressedEvent += PresentBeatLeaderInfoModal;
            LeaderboardEvents.VotingWasPressedEvent += PresentVotingModal;
            LeaderboardEvents.ContextSelectorWasPressedAction += PresentContextsModal;
            LeaderboardState.IsVisible = true;
        }

        protected void OnDisable() {
            LeaderboardEvents.ScoreInfoButtonWasPressed -= PresentScoreInfoModal;
            LeaderboardEvents.LeaderboardSettingsButtonWasPressedEvent -= PresentSettingsModal;
            LeaderboardEvents.LogoWasPressedEvent -= PresentBeatLeaderInfoModal;
            LeaderboardEvents.VotingWasPressedEvent -= PresentVotingModal;
            LeaderboardEvents.ContextSelectorWasPressedAction -= PresentContextsModal;
            LeaderboardState.IsVisible = false;
        }

        #endregion

        #region Events

        private void PresentScoreInfoModal(Score score) {
            ReeModalSystem.OpenModal<ScoreInfoPanel>(transform, (score, _replayerStarter));
        }

        private void PresentSettingsModal() {
            ReeModalSystem.OpenModal<LeaderboardSettings>(transform, 0);
        }

        private void PresentBeatLeaderInfoModal() {
            ReeModalSystem.OpenModal<BeatLeaderInfo>(transform, 0);
        }

        private void PresentVotingModal() {
            ReeModalSystem.OpenModal<VotingPanel>(transform, 0);
        }

        private void PresentContextsModal() {
            ReeModalSystem.OpenModal<ContextsModal>(transform, 0);
        }

        #endregion
    }
}
using System;
using BeatLeader.DataManager;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, IInitializable, IDisposable 
    {
        #region Components

        [UIValue("score-details"), UsedImplicitly]
        private ScoreDetails _scoreDetails = ReeUIComponent.Instantiate<ScoreDetails>();

        [UIValue("scores-table"), UsedImplicitly]
        private ScoresTable _scoresTable = ReeUIComponent.Instantiate<ScoresTable>(false);

        [UIValue("pagination"), UsedImplicitly]
        private Pagination _pagination = ReeUIComponent.Instantiate<Pagination>();

        [UIValue("scope-selector"), UsedImplicitly]
        private ScopeSelector _scopeSelector = ReeUIComponent.Instantiate<ScopeSelector>();

        [UIValue("context-selector"), UsedImplicitly]
        private ContextSelector _contextSelector = ReeUIComponent.Instantiate<ContextSelector>();

        [UIValue("empty-board-message"), UsedImplicitly]
        private EmptyBoardMessage _emptyBoardMessage = ReeUIComponent.Instantiate<EmptyBoardMessage>();

        #endregion

        #region Initialize/Dispose

        public void Initialize() {
            LeaderboardEvents.ScoreInfoButtonWasPressed += OnScoreInfoButtonWasPressed;
        }

        public void Dispose() {
            LeaderboardEvents.ScoreInfoButtonWasPressed -= OnScoreInfoButtonWasPressed;
        }

        #endregion

        #region Events

        private void OnScoreInfoButtonWasPressed(Score score) 
        {
            _scoreDetails.SetScore(score);
            UpdateReplayData();
            ShowScoreModal();
        }

        #endregion

        #region ReplayData

        private void UpdateReplayData()
        {
            ReplayDataManager.GetReplay();
        }

        #endregion

        #region OnEnable & OnDisable

        protected void OnEnable() {
            LeaderboardEvents.NotifyIsLeaderboardVisibleChanged(true);
        }

        protected void OnDisable() {
            LeaderboardEvents.NotifyIsLeaderboardVisibleChanged(false);
            HideScoreModal();
        }

        #endregion

        #region ScoresModal

        [UIComponent("scores-modal"), UsedImplicitly]
        private ModalView _scoresModal;

        private void ShowScoreModal() {
            if (_scoresModal == null) return;
            _scoresModal.Show(true, true);
        }

        private void HideScoreModal() {
            if (_scoresModal == null) return;
            _scoresModal.Hide(true);
        }

        #endregion
    }
}
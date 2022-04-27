using System;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using Zenject;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, IInitializable, IDisposable {
        #region Components

        [UIValue("score-details"), UsedImplicitly]
        private ScoreInfoPanel _scoreInfoPanel = ReeUIComponent.Instantiate<ScoreInfoPanel>();

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
            LeaderboardEvents.SceneTransitionStartedEvent += OnSceneTransitionStarted;
        }

        public void Dispose() {
            LeaderboardEvents.ScoreInfoButtonWasPressed -= OnScoreInfoButtonWasPressed;
            LeaderboardEvents.SceneTransitionStartedEvent -= OnSceneTransitionStarted;
        }

        #endregion

        #region Events

        private void OnScoreInfoButtonWasPressed(Score score) {
            _scoreInfoPanel.SetScore(score);
            ShowScoreModal();
        }

        private void OnSceneTransitionStarted() {
            HideScoreModal(false);
        }

        #endregion

        #region OnEnable & OnDisable

        protected void OnEnable() {
            LeaderboardState.IsVisible = true;
        }

        protected void OnDisable() {
            LeaderboardState.IsVisible = false;
            HideScoreModal(true);
        }

        #endregion

        #region ScoresModal

        private static readonly Vector3 ModalOffset = new(0.0f, -0.6f, 0.0f);

        [UIComponent("scores-modal"), UsedImplicitly]
        private ModalView _scoresModal;

        private bool _modalVisible;

        private void ShowScoreModal() {
            if (_modalVisible || _scoresModal == null) return;
            _scoresModal.Show(true, true);
            _scoresModal.transform.position += ModalOffset;
            _modalVisible = true;
        }

        private void HideScoreModal(bool animated) {
            if (!_modalVisible || _scoresModal == null) return;
            _scoresModal.Hide(animated);
            _modalVisible = false;
        }

        #endregion
    }
}
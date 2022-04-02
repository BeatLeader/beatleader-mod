using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.EmptyBoardMessage.bsml")]
    internal class EmptyBoardMessage : ReeUIComponent {
        #region OnInitialize

        protected override void OnInitialize() {
            LeaderboardEvents.ScoresRequestStartedEvent += OnScoresRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent += OnScoresFetched;
            LeaderboardEvents.ScoresFetchFailedEvent += OnScoresRequestFailed;
            ApplyAlpha();
        }

        protected override void OnDispose() {
            LeaderboardEvents.ScoresRequestStartedEvent -= OnScoresRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent -= OnScoresFetched;
            LeaderboardEvents.ScoresFetchFailedEvent -= OnScoresRequestFailed;
        }

        #endregion

        #region Events

        private const string GoodNewsColor = "#88FF88";
        private const string BadNewsColor = "#FF8888";

        private void OnScoresRequestStarted() {
            FadeOut();
        }

        private void OnScoresFetched(Paged<Score> scoreData) {
            if (scoreData.data.IsEmpty()) {
                SetText(
                    GoodNewsColor,
                    "No scores yet",
                    "Be the first!"
                );
                FadeIn();
            } else {
                FadeOut();
            }
        }

        private void OnScoresRequestFailed() {
            SetText(
                BadNewsColor,
                "Oops!",
                "An unexpected error occured"
            );
            FadeIn();
        }

        #endregion

        #region SetText

        private void SetText(string color, string header, string message) {
            MessageText = $"<color={color}>{header}\n<size=70%>{message}";
        }

        #endregion

        #region Animation

        private const float FadeSpeed = 12.0f;
        private float _currentAlpha;
        private float _targetAlpha;

        private void FadeIn() {
            _targetAlpha = 1.0f;
        }

        private void FadeOut() {
            _targetAlpha = 0.0f;
        }

        private void LateUpdate() {
            var t = Time.deltaTime * FadeSpeed;
            LerpAlpha(t);
        }

        private void LerpAlpha(float t) {
            if (_currentAlpha.Equals(_targetAlpha)) return;
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
            ApplyAlpha();
        }

        private void ApplyAlpha() {
            _textComponent.color = _textComponent.color.ColorWithAlpha(_currentAlpha);
        }

        #endregion

        #region Text

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        private string _messageText = "";

        [UIValue("message-text"), UsedImplicitly]
        private string MessageText {
            get => _messageText;
            set {
                if (_messageText.Equals(value)) return;
                _messageText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}
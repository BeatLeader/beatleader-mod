using BeatLeader.API;
using BeatLeader.WebRequests;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class EmptyBoardMessage : ReeUIComponentV2 {
        #region OnInitialize

        protected override void OnInitialize() {
            ScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            ClanScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            ApplyAlpha();
        }

        protected override void OnDispose() {
            ScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
            ClanScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
        }

        #endregion

        #region Events

        private const string GoodNewsColor = "#88FF88";
        private const string BadNewsColor = "#FF8888";
        private const string DefaultErrorMessage = "An unexpected error occured";

        private void OnScoresRequestStateChanged(IWebRequest<ScoresTableContent> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Started:
                    FadeOut();
                    break;
                case WebRequests.RequestState.Failed:
                    ShowError(failReason);
                    break;
                case WebRequests.RequestState.Finished:
                    OnScoresFetched(instance.Result);
                    break;
            }
        }

        private void OnScoresFetched(ScoresTableContent scoreData) {
            if (scoreData.MainRowContents.IsEmpty()) {
                ShowEmpty();
                return;
            }

            FadeOut();
        }

        private void ShowEmpty() {
            SetText(
                GoodNewsColor,
                "No scores yet",
                "Be the first!"
            );
            FadeIn();
        }

        private void ShowError(string reason) {
            SetText(
                BadNewsColor,
                "Oops!",
                reason.IsEmpty() ? DefaultErrorMessage : reason.Truncate(50, true)
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
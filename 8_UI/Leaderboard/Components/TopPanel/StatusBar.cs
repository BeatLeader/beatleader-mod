using System;
using System.Collections;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class StatusBar : ReeUIComponentV2 {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.StatusMessageEvent += OnStatusMessage;
            LeaderboardState.ProfileRequest.FailedEvent += OnProfileRequestFailed;
            LeaderboardState.UploadRequest.FinishedEvent += OnScoreUploadSuccess;
            LeaderboardState.UploadRequest.FailedEvent += OnScoreUploadFailed;
            LeaderboardState.VoteRequest.FinishedEvent += OnVoteFinished;
            LeaderboardState.VoteRequest.FailedEvent += OnVoteFailed;
        }

        protected override void OnDispose() {
            LeaderboardEvents.StatusMessageEvent -= OnStatusMessage;
            LeaderboardState.ProfileRequest.FailedEvent -= OnProfileRequestFailed;
            LeaderboardState.UploadRequest.FinishedEvent -= OnScoreUploadSuccess;
            LeaderboardState.UploadRequest.FailedEvent -= OnScoreUploadFailed;
            LeaderboardState.VoteRequest.FinishedEvent -= OnVoteFinished;
            LeaderboardState.VoteRequest.FailedEvent -= OnVoteFailed;
        }

        private void OnDisable() {
            StopAllCoroutines();
            MessageText = "";
        }

        #endregion

        #region Events
        
        private void OnVoteFinished(VoteStatus result) {
            ShowGoodNews("Your vote has been accepted!");
        }

        private void OnVoteFailed(string reason) {
            ShowBadNews($"Vote failed! {reason}");
        }

        private void OnProfileRequestFailed(string reason) {
            ShowBadNews($"Profile update failed! {reason}");
        }

        private void OnScoreUploadFailed(string reason) {
            ShowBadNews($"Score upload failed! {reason}");
        }

        private void OnScoreUploadSuccess(Score score) {
            ShowGoodNews("Score uploaded!");
        }

        private void OnStatusMessage(string message, LeaderboardEvents.StatusMessageType type, float duration) {
            switch (type) {
                case LeaderboardEvents.StatusMessageType.Neutral: 
                    ShowMessage(message, duration);
                    break;
                case LeaderboardEvents.StatusMessageType.Bad:
                    ShowBadNews(message, duration);
                    break;
                case LeaderboardEvents.StatusMessageType.Good:
                    ShowGoodNews(message, duration);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion

        #region ShowMessage

        private const float DefaultDuration = 1.4f;
        private const string GoodNewsColor = "#88FF88";
        private const string BadNewsColor = "#FF8888";

        private void ShowGoodNews(string message, float duration = DefaultDuration) {
            ShowMessage($"<color={GoodNewsColor}>{message}", duration);
        }

        private void ShowBadNews(string message, float duration = DefaultDuration) {
            ShowMessage($"<color={BadNewsColor}>{message}", duration);
        }

        private void ShowMessage(string message, float duration = DefaultDuration) {
            if (!gameObject.activeInHierarchy) return;
            StopAllCoroutines();
            StartCoroutine(ShowMessageCoroutine(message, duration));
        }

        private IEnumerator ShowMessageCoroutine(string message, float duration) {
            MessageText = message;
            yield return new WaitForSeconds(duration);
            MessageText = "";
        }

        #endregion

        #region MessageText

        private string _messageText = "";

        [UIValue("message-text"), UsedImplicitly]
        public string MessageText {
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
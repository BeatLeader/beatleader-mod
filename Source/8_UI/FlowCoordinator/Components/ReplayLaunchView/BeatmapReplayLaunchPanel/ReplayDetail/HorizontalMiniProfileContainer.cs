using System;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class HorizontalMiniProfileContainer : ReeUIComponentV2 {
        #region Events

        public event Action<Player>? PlayerLoadedEvent; 

        #endregion
        
        #region UI Components

        [UIValue("mini-profile"), UsedImplicitly]
        private HorizontalMiniProfile _horizontalMiniProfile = null!;

        [UIObject("loading-container")]
        private readonly GameObject _loadingContainerObject = null!;

        [UIObject("mini-profile-container")]
        private readonly GameObject _miniProfileContainerObject = null!;

        #endregion

        #region Text

        private const string LoadingMessage = "Loading...";
        private const string FailedMessage = "Player with such id doesn't exists";
        private const string NotSelectedMessage = "Replay is not selected";

        [UIValue("loading-text"), UsedImplicitly]
        private string LoadingText {
            get => _loadingText;
            set {
                _loadingText = value;
                NotifyPropertyChanged();
            }
        }

        private string _loadingText = LoadingMessage;

        #endregion

        #region SetPlayer

        private string? _playerId = string.Empty;
        
        public void SetPlayer(string? playerId) {
            if (_playerId == playerId) return;
            _playerId = playerId;
            if (playerId is not null) {
                PlayerRequest.SendRequest(playerId);
                return;
            }
            _loadingContainerObject.SetActive(true);
            _miniProfileContainerObject.SetActive(false);
            LoadingText = NotSelectedMessage;
        }

        #endregion

        #region Init

        protected override void OnInstantiate() {
            _horizontalMiniProfile = Instantiate<HorizontalMiniProfile>(transform);
            PlayerRequest.AddStateListener(HandlePlayerRequestStateChanged);
        }

        protected override void OnDispose() {
            PlayerRequest.RemoveStateListener(HandlePlayerRequestStateChanged);
        }

        #endregion

        #region Callbacks

        private void HandlePlayerRequestStateChanged(API.RequestState state, Player result, string failReason) {
            if (state is API.RequestState.Uninitialized) return;
            var showLoading = state is API.RequestState.Started;
            _loadingContainerObject.SetActive(showLoading);
            _miniProfileContainerObject.SetActive(!showLoading);
            LoadingText = state is not API.RequestState.Failed ? LoadingMessage : FailedMessage;
            if (state is not API.RequestState.Finished) return;
            _horizontalMiniProfile.SetPlayer(result);
            PlayerLoadedEvent?.Invoke(result);
        }

        #endregion
    }
}
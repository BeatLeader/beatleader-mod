using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class HorizontalMiniProfileContainer : ReeUIComponentV2 {
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
        private const string NotSelectedMessage = "Unknown player";

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

        public Player? Player { get; private set; }

        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _isWorking;
        private string? _playerId = string.Empty;

        public async Task SetPlayer(string? playerId) {
            if (_playerId == playerId) return;
            if (_isWorking) {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new();
            }
            _playerId = playerId;
            _isWorking = true;
            if (playerId is not null) {
                await GetPlayerAsync(playerId, _cancellationTokenSource.Token);
                return;
            }
            UpdateVisibility(State.LoadFailed);
            Player = null;
        }

        #endregion

        #region UpdateVisibility

        private enum State {
            Loading,
            Loaded,
            LoadFailed
        }

        private void UpdateVisibility(State state) {
            switch (state) {
                case State.LoadFailed:
                case State.Loading:
                    _loadingContainerObject.SetActive(true);
                    _miniProfileContainerObject.SetActive(false);
                    LoadingText = state is State.Loading ? LoadingMessage : NotSelectedMessage;
                    break;
                case State.Loaded:
                    _loadingContainerObject.SetActive(false);
                    _miniProfileContainerObject.SetActive(true);
                    break;
            }
        }

        #endregion

        #region Web Requests

        private static string PlayerEndpoint => BLConstants.BEATLEADER_API_URL + "/player/";

        private async Task GetPlayerAsync(string playerId, CancellationToken token) {
            UpdateVisibility(State.Loading);
            var player = await WebUtils.SendAndDeserializeAsync<Player>(PlayerEndpoint + playerId);
            if (token.IsCancellationRequested) return;
            UpdateVisibility(player is null ? State.LoadFailed : State.Loaded);
            if (player is not null) _horizontalMiniProfile.SetPlayer(player);
            Player = player;
            _isWorking = false;
        }

        #endregion

        #region Init

        protected override void OnInstantiate() {
            _horizontalMiniProfile = Instantiate<HorizontalMiniProfile>(transform);
        }
        
        #endregion
    }
}
﻿using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;

namespace BeatLeader.Components {
    internal class MiniProfile : ReeUIComponentV2 {
        #region Components

        [UIValue("player-avatar"), UsedImplicitly]
        private PlayerAvatar _playerAvatar;

        [UIValue("mini-profile-buttons"), UsedImplicitly]
        private MiniProfileButtons _miniProfileButtons;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _countryFlag;

        private void Awake() {
            _playerAvatar = Instantiate<PlayerAvatar>(transform);
            _miniProfileButtons = Instantiate<MiniProfileButtons>(transform);
            _countryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion

        #region SetPlayer

        private Player _player;

        public void SetPlayer(Player player) {
            _player = player;
            _miniProfileButtons.SetPlayer(_player);
            _playerAvatar.SetAvatar(_player.avatar, _player.profileSettings);
            _countryFlag.SetCountry(_player.country);
            SetMessage(_player.profileSettings?.message ?? "");

            PlayerName = FormatUtils.FormatUserName(_player.name);
            PlayerGlobalRank = FormatUtils.FormatRank(_player.rank, true);
            PlayerCountryRank = FormatUtils.FormatRank(_player.countryRank, true);
            PlayerPp = FormatUtils.FormatPP(_player.pp);
        }

        #endregion

        #region PatreonFeatures

        private const float NoMessageOffset = 33.5f;
        private const float MessageOffset = NoMessageOffset + 7;

        private void SetMessage(string message) {
            try {
                MessageText = message;
            } catch (Exception) {
                MessageText = "";
            }

            ShowMessage = !message.IsEmpty();
            YOffset = ShowMessage ? MessageOffset : NoMessageOffset;
        }

        #endregion

        #region playerName

        private string _playerName = "";

        [UIValue("player-name"), UsedImplicitly]
        public string PlayerName {
            get => _playerName;
            set {
                if (_playerName.Equals(value)) return;
                _playerName = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PlayerGlobalRank

        private string _playerGlobalRank = "";

        [UIValue("player-global-rank"), UsedImplicitly]
        public string PlayerGlobalRank {
            get => _playerGlobalRank;
            set {
                if (_playerGlobalRank.Equals(value)) return;
                _playerGlobalRank = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region PlayerCountryRank

        private string _playerCountryRank = "";

        [UIValue("player-country-rank"), UsedImplicitly]
        public string PlayerCountryRank {
            get => _playerCountryRank;
            set {
                if (_playerCountryRank.Equals(value)) return;
                _playerCountryRank = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region playerPp

        private string _playerPp = "";

        [UIValue("player-pp"), UsedImplicitly]
        public string PlayerPp {
            get => _playerPp;
            set {
                if (_playerPp.Equals(value)) return;
                _playerPp = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region YOffset

        private float _yOffset;

        [UIValue("y-offset"), UsedImplicitly]
        public float YOffset {
            get => _yOffset;
            set {
                if (_yOffset.Equals(value)) return;
                _yOffset = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Message

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

        private bool _showMessage;

        [UIValue("show-message"), UsedImplicitly]
        public bool ShowMessage {
            get => _showMessage;
            set {
                if (_showMessage.Equals(value)) return;
                _showMessage = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}
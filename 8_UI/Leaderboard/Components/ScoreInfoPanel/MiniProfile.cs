using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class MiniProfile : ReeUIComponentV2 {
        #region Components

        [UIValue("player-avatar"), UsedImplicitly]
        private PlayerAvatar _playerAvatar;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _countryFlag;

        private void Awake() {
            _playerAvatar = Instantiate<PlayerAvatar>(transform);
            _countryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion

        #region SetPlayer

        public void SetPlayer(Player player) {
            _playerAvatar.SetAvatar(player.avatar, FormatUtils.ParseMostSignificantRole(player.role));
            _countryFlag.SetCountry(player.country);

            PlayerName = FormatUtils.FormatUserName(player.name);
            PlayerGlobalRank = FormatUtils.FormatRank(player.rank, true);
            PlayerCountryRank = FormatUtils.FormatRank(player.countryRank, true);
            PlayerPp = FormatUtils.FormatPP(player.pp);
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
    }
}
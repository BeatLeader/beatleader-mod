using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;

namespace BeatLeader.Components {
    internal class HorizontalMiniProfile : EditableElement {
        #region Components

        [UIValue("player-avatar")] private PlayerAvatar _playerAvatar;
        [UIValue("country-flag")] private CountryFlag _playerCountryFlag;

        #endregion

        #region Name, Rank, PP

        [UIValue("player-name")]
        public string PlayerName {
            get => _playerName;
            private set {
                _playerName = value;
                NotifyPropertyChanged(nameof(PlayerName));
            }
        }
        [UIValue("player-global-rank")]
        public string PlayerGlobalRank {
            get => _playerGlobalRank;
            private set {
                _playerGlobalRank = value;
                NotifyPropertyChanged(nameof(PlayerGlobalRank));
            }
        }
        [UIValue("player-pp")]
        public string PlayerPp {
            get => _playerPp;
            private set {
                _playerPp = value;
                NotifyPropertyChanged(nameof(PlayerPp));
            }
        }

        #endregion

        #region Editable
        public override string Name => "Mini Profile";
        public override LayoutMapData DefaultLayoutMap { get; protected set; } = new() {
            enabled = true,
            position = new(0f, 0.85f),
            anchor = new(0f, 0f)
        };

        #endregion

        #region Setup

        private string _playerName;
        private string _playerGlobalRank;
        private string _playerPp;

        public void SetPlayer(Player player) {
            _playerAvatar.SetPlayer(player);
            _playerCountryFlag.SetCountry(player.country);

            PlayerName = player.name;
            PlayerGlobalRank = FormatUtils.FormatRank(player.rank, true);
            PlayerPp = FormatUtils.FormatPP(player.pp);
        }

        protected override void OnInstantiate() {
            _playerAvatar = Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion
    }
}

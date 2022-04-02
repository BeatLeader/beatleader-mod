using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreDetails.bsml")]
    internal class ScoreDetails : ReeUIComponent {
        #region Components

        [UIValue("player-avatar"), UsedImplicitly]
        private PlayerAvatar _playerAvatar = Instantiate<PlayerAvatar>();

        #endregion

        #region SetScore

        public void SetScore(Score score) {
            PlayerName = FormatUtils.FormatUserName(score.player.name);
            PlayerRank = FormatUtils.FormatRank(score.player.rank, true);
            PlayerPp = FormatUtils.FormatPP(score.player.pp);

            _playerAvatar.SetAvatar(score.player.avatar);
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

        #region playerRank

        private string _playerRank = "";

        [UIValue("player-global-rank"), UsedImplicitly]
        public string PlayerRank {
            get => _playerRank;
            set {
                if (_playerRank.Equals(value)) return;
                _playerRank = value;
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
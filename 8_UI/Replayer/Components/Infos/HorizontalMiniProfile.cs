using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class HorizontalMiniProfile : EditableElement
    {
        #region Components

        [UIValue("player-avatar")] private PlayerAvatar _playerAvatar;
        [UIValue("country-flag")] private CountryFlag _playerCountryFlag;

        [UIComponent("container")] private readonly RectTransform _container;
        [UIComponent("wrapper")] private readonly RectTransform _wrapper;

        #endregion

        #region Name, Rank, PP

        [UIValue("player-name")] public string PlayerName
        {
            get => _playerName;
            private set
            {
                _playerName = value;
                NotifyPropertyChanged(nameof(PlayerName));
            }
        }
        [UIValue("player-global-rank")] public string PlayerGlobalRank
        {
            get => _playerGlobalRank;
            private set
            {
                _playerGlobalRank = value;
                NotifyPropertyChanged(nameof(PlayerGlobalRank));
            }
        }
        [UIValue("player-pp")] public string PlayerPp
        {
            get => _playerPp;
            private set
            {
                _playerPp = value;
                NotifyPropertyChanged(nameof(PlayerPp));
            }
        }

        #endregion

        #region Editable

        protected override RectTransform ContainerRect => _container;
        protected override RectTransform WrapperRect => _wrapper;
        protected override HideMode Mode => HideMode.Opacity;
        public override string Name => "Player Info";

        #endregion

        #region Setup

        private string _playerName;
        private string _playerGlobalRank;
        private string _playerPp;

        public void SetPlayer(Player player)
        {
            _playerAvatar.SetPlayer(player);
            _playerCountryFlag.SetCountry(player.country);

            PlayerName = player.name;
            PlayerGlobalRank = FormatUtils.FormatRank(player.rank, true);
            PlayerPp = FormatUtils.FormatPP(player.pp);
        }

        protected override void OnInstantiate()
        {
            _playerAvatar = Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = Instantiate<CountryFlag>(transform);
        }

        #endregion
    }
}

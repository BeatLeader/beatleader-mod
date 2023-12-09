using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class QuickMiniProfile : ReeUIComponentV3<QuickMiniProfile> {
        #region Components

        [UIValue("player-avatar"), UsedImplicitly]
        private PlayerAvatar _playerAvatar = null!;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _playerCountryFlag = null!;

        [UIObject("text-container"), UsedImplicitly]
        private GameObject _textContainer = null!;
        
        private RectTransform _avatarRectTransform = null!;
        private LayoutElement _avatarLayoutElement = null!;
        
        #endregion

        #region Values

        [UIValue("player-name")]
        public string? PlayerName {
            get => _playerName;
            private set {
                _playerName = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("player-global-rank")]
        public string? PlayerGlobalRank {
            get => _playerGlobalRank;
            private set {
                _playerGlobalRank = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("player-pp")]
        public string? PlayerPp {
            get => _playerPp;
            private set {
                _playerPp = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Setup

        private string? _playerName;
        private string? _playerGlobalRank;
        private string? _playerPp;

        public void SetPlayer(IPlayer player) {
            _playerAvatar.SetPlayer(player);
            _playerCountryFlag.SetCountry(player.Country);

            PlayerName = player.Name;
            PlayerGlobalRank = FormatUtils.FormatRank(player.Rank, true);
            PlayerPp = FormatUtils.FormatPP(player.PerformancePoints);
        }

        protected override void OnInstantiate() {
            _playerAvatar = ReeUIComponentV2.Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = ReeUIComponentV2.Instantiate<CountryFlag>(transform);
        }

        protected override void OnInitialize() {
            _avatarRectTransform = (RectTransform)_playerAvatar.GetRootTransform();
            //i could use AspectRatioFitter, but it does not work properly with flexible values
            _avatarLayoutElement = _avatarRectTransform.gameObject.GetComponent<LayoutElement>();
        }

        protected override void OnStart() {
            RecalculateLayout();
        }

        #endregion

        #region Layout

        protected override void OnRectDimensionsChange() {
            RecalculateLayout();
        }

        private void RecalculateLayout() {
            //avatar has 1:1 aspect ratio, so if height is 10, then width is 10 too. So, we can simply subtract the height
            var rect = _avatarRectTransform.rect;
            _avatarLayoutElement.preferredWidth = rect.height;
            _avatarLayoutElement.minWidth = rect.height;
        }

        #endregion
    }
}
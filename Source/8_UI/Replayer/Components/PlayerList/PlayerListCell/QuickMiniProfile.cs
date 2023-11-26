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

        [UIComponent("text-container"), UsedImplicitly]
        private LayoutElement _textContainerLayoutElement = null!;

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

        public void SetPlayer(Player player) {
            _playerAvatar.SetPlayer(player);
            _playerCountryFlag.SetCountry(player.country);

            PlayerName = player.name;
            PlayerGlobalRank = FormatUtils.FormatRank(player.rank, true);
            PlayerPp = FormatUtils.FormatPP(player.pp);
        }

        protected override void OnInstantiate() {
            _playerAvatar = ReeUIComponentV2.Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = ReeUIComponentV2.Instantiate<CountryFlag>(transform);
        }

        protected override void OnInitialize() {
            var go = _playerAvatar.GetRootTransform().gameObject;
            var fitter = go.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            RecalculateLayout();
        }

        private void Start() {
            RecalculateLayout();
        }

        #endregion

        #region Layout

        private void RecalculateLayout() {
            if (ContentTransform is not RectTransform rectTransform) return;
            var rect = rectTransform.rect;
            //avatar has 1:1 aspect ratio, so if the height is 10, then width is 10 too. So, we can simply subtract the height
            _textContainerLayoutElement.preferredWidth = rect.width - rect.height;
        }

        #endregion
    }
}
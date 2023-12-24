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

        public void SetPlayer(IPlayer? player) {
            _playerAvatar.SetPlayer(player);
            _playerCountryFlag.SetCountry(player?.Country ?? "not set");

            PlayerName = player?.Name ?? "Loading...";
            PlayerGlobalRank = FormatUtils.FormatRank(player?.Rank ?? -1, true);
            PlayerPp = FormatUtils.FormatPP(player?.PerformancePoints ?? -1);
        }

        protected override void OnInstantiate() {
            _playerAvatar = ReeUIComponentV2.Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = ReeUIComponentV2.Instantiate<CountryFlag>(transform);

            var playerAvatarGo = _playerAvatar.GetRootTransform().gameObject;
            playerAvatarGo.AddComponent<FlexItem>();
            var aspectFitter = playerAvatarGo.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        }

        #endregion
    }
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class HorizontalMiniProfileHost : INotifyPropertyChanged {
        #region Components

        [UIValue("player-avatar")]
        private PlayerAvatar _playerAvatar = null!;

        [UIValue("country-flag")]
        private CountryFlag _playerCountryFlag = null!;

        [UIObject("flag-name-container")]
        private readonly GameObject _flagNameContainer = null!;

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

        [UIValue("text-container-size"), UsedImplicitly]
        private float TextContainerSize => TextBgSize * 2 + 1;

        [UIValue("text-bg-size"), UsedImplicitly]
        private float TextBgSize => textSize + 2;

        [UIValue("max-text-width"), UsedImplicitly]
        private float MaxTextWidth => maxWidth - avatarSize;

        [UIValue("text-size"), UsedImplicitly]
        public float textSize = 4;

        [UIValue("avatar-size"), UsedImplicitly]
        public float avatarSize = 20;

        [UIValue("ignore-layout"), UsedImplicitly]
        public bool ignoreLayout = true;

        [UIValue("max-width")]
        public float maxWidth = -1;

        #endregion

        #region Setup

        private string? _playerName;
        private string? _playerGlobalRank;
        private string? _playerPp;

        public void SetPlayer(Player player) {
            _playerAvatar.SetAvatar(player.avatar, player.profileSettings);
            _playerCountryFlag.SetCountry(player.country);

            PlayerName = player.name;
            PlayerGlobalRank = FormatUtils.FormatRank(player.rank, true);
            PlayerPp = FormatUtils.FormatPP(player.pp);
        }

        public void OnInstantiate(Transform transform) {
            _playerAvatar = ReeUIComponentV2.Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = ReeUIComponentV2.Instantiate<CountryFlag>(transform);
        }

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            if (maxWidth is -1) return;
            _flagNameContainer.GetComponent<LayoutElement>().TryDestroy();
            _flagNameContainer.AddComponent<AdvancedLayoutElement>().maxWidth = 800;
        }

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Replayer.Components.Infos.HorizontalMiniProfile.bsml")]
    internal class HorizontalMiniProfileLayoutComponent : LayoutEditorComponent<HorizontalMiniProfileLayoutComponent> {
        #region Components

        [UIValue("player-avatar"), UsedImplicitly]
        private PlayerAvatar _playerAvatar = null!;

        [UIValue("country-flag"), UsedImplicitly]
        private CountryFlag _playerCountryFlag = null!;

        [UIObject("flag-name-container"), UsedImplicitly]
        private readonly GameObject _flagNameContainer = null!;

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

        [UIValue("text-container-size"), UsedImplicitly]
        private float TextContainerSize => TextBgSize * 2 + 1;

        [UIValue("text-bg-size"), UsedImplicitly]
        private float TextBgSize => textSize + 2;
        
        [UIValue("max-text-width"), UsedImplicitly]
        private float MaxTextWidth => maxWidth - avatarSize;
        
        [UIValue("text-size"), UsedImplicitly]
        public float textSize = 4;
        
        [UIValue("avatar-size"), UsedImplicitly]
        public float avatarSize = 20;

        [UIValue("ignore-layout"), UsedImplicitly]
        public bool ignoreLayout = true;

        [UIValue("max-width")]
        public float maxWidth = -1;
        
        #endregion
        
        #region LayoutComponent

        public override string ComponentName => "Mini Profile";
        protected override Vector2 MinSize { get; } = new(0, 24);
        protected override Vector2 MaxSize { get; } = new(int.MaxValue, 24);

        #endregion

        #region Setup

        protected override string Markup { get; } = typeof(HorizontalMiniProfileLayoutComponent).ReadViewDefinition();
        
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
            if (maxWidth is -1) return;
            _flagNameContainer.GetComponent<LayoutElement>().TryDestroy();
            _flagNameContainer.AddComponent<AdvancedLayoutElement>().maxWidth = 800;
        }
        
        #endregion
    }

    internal class HorizontalMiniProfile : ReeUIComponentV2 {
        #region Setup

        protected override object ParseHost => _host;

        private readonly HorizontalMiniProfileHost _host = new();

        public void SetPlayer(Player player) {
            _host.SetPlayer(player);
        }

        protected override void OnInstantiate() {
            base.OnInstantiate();
            _host.avatarSize *= 0.6f;
            _host.textSize *= 0.6f;
            _host.maxWidth = 48;
            _host.ignoreLayout = false;
            _host.OnInstantiate(transform);
        }

        #endregion
    }
}
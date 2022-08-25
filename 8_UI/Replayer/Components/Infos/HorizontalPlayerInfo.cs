using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components
{
    internal class HorizontalPlayerInfo : EditableElement
    {
        [Inject] private readonly ReplayLaunchData _replayData;
        [InjectOptional] private readonly LayoutEditor _layoutEditor;

        [UIValue("player-avatar")] private PlayerAvatar _playerAvatar;
        [UIValue("country-flag")] private CountryFlag _playerCountryFlag;
        [UIComponent("container")] private RectTransform _container;
        [UIComponent("wrapper")] private RectTransform _wrapper;

        [UIValue("player-name")] private string _playerName => _player.name;
        [UIValue("player-global-rank")] private string _playerGlobalRank => FormatUtils.FormatRank(_player.rank, true);
        [UIValue("player-pp")] private string _playerPp => FormatUtils.FormatPP(_player.pp);

        protected override RectTransform ContainerRect => _container;
        protected override RectTransform WrapperRect => _wrapper;
        protected override HideMode Mode => HideMode.Hierarchy;
        public override string Name => "PlayerInfo";

        private Player _player => _replayData.player;

        protected override void OnInstantiate()
        {
            _playerAvatar = Instantiate<PlayerAvatar>(transform);
            _playerCountryFlag = Instantiate<CountryFlag>(transform);
        }
        protected override void OnInitialize()
        {
            _playerAvatar.SetAvatar(_player.avatar, FormatUtils.ParsePlayerRoles(_player.role));
            _playerCountryFlag.SetCountry(_player.country);
            _layoutEditor?.TryAddObject(this);
        }
    }
}

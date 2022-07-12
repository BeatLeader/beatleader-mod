using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatLeader.Components;
using Player = BeatLeader.Models.Player;
using UnityEngine.UI;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class PlayersInfoList : ReeUIComponentV2
    {
        [UIComponent("players-infos-list")] 
        private readonly CustomCellListTableData _playersInfosList;

        public void AddPlayer(Player player)
        {
            RegisterCell(new PlayerInfoCell(player));
        }
        public void AddPlayers(List<Player> players)
        {
            players.ForEach(x => AddPlayer(x));
        }
        protected void RegisterCell(PlayerInfoCell cell)
        {
            _playersInfosList.data.Add(cell);
            _playersInfosList.tableView.ReloadData();
            cell.OnCellRegistered();
        }

        protected class PlayerInfoCell
        {
            [UIValue("player-avatar")] 
            public readonly PlayerAvatar playerAvatar;

            [UIValue("player-country-flag")] 
            public readonly CountryFlag playerCountryFlag;

            [UIValue("player-name")] 
            public readonly string playerName;

            public readonly Player player;

            public PlayerInfoCell(Player player)
            {
                this.player = player;
                playerName = player.name.Length <= 17 ? player.name :
                    player.name.Remove(15, player.name.Length - 15) + "...";
                playerAvatar = Instantiate<PlayerAvatar>(null);
                playerCountryFlag = Instantiate<CountryFlag>(null);
            }
            public void OnCellRegistered()
            {
                playerAvatar.SetAvatar(player.avatar, FormatUtils.ParsePlayerRoles(player.role));
                playerCountryFlag.SetCountry(player.country);
            }
        }
    }
}

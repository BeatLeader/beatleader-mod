using System;
using BeatLeader.Components;
using BeatLeader.DataManager;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class ClanPlayer : IScoreRowContent {
        [JsonProperty("player")]
        public Player Player { 
            get {
                if (!HiddenPlayersCache.IsHidden(_originalPlayer)) return _originalPlayer;
            
                return new Player() {
                    id = _originalPlayer.id,
                    rank = 0,
                    name = "~hidden player~",
                    country = "not set",
                    countryRank = 0,
                    pp = 0f,
                    role = "",
                    clans = Array.Empty<Clan>(),
                    socials = Array.Empty<ServiceIntegration>(),
                    profileSettings = null
                };
            }
            set { 
                _originalPlayer = value;
            }
        }
        
        public Score? score;
        public Player _originalPlayer;

        #region IScoreRowContent implementation

        public bool ContainsValue(ScoreRowCellType cellType) {
            return cellType switch {
                ScoreRowCellType.Rank => true,
                ScoreRowCellType.Country => true,
                ScoreRowCellType.Avatar => true,
                ScoreRowCellType.Username => true,
                ScoreRowCellType.Modifiers => true,
                ScoreRowCellType.Accuracy => true,
                ScoreRowCellType.PerformancePoints => true,
                ScoreRowCellType.Score => true,
                ScoreRowCellType.Mistakes => true,
                ScoreRowCellType.Clans => false,
                ScoreRowCellType.Time => true,
                ScoreRowCellType.Pauses => true,
                _ => false
            };
        }

        public object? GetValue(ScoreRowCellType cellType) {
            return cellType switch {
                ScoreRowCellType.Rank => Player.rank,
                ScoreRowCellType.Country => Player.country,
                ScoreRowCellType.Avatar => new AvatarScoreRowCell.Data(Player.avatar, Player.profileSettings),
                ScoreRowCellType.Username => Player.name,
                ScoreRowCellType.Modifiers => score?.modifiers,
                ScoreRowCellType.Accuracy => score?.accuracy,
                ScoreRowCellType.PerformancePoints => score?.pp,
                ScoreRowCellType.Score => score?.modifiedScore,
                ScoreRowCellType.Mistakes => score?.TotalMistakes,
                ScoreRowCellType.Clans => Player.clans,
                ScoreRowCellType.Time => score?.timeSet,
                ScoreRowCellType.Pauses => score?.pauses,
                _ => default
            };
        }

        #endregion
    }
}
using BeatLeader.Components;
using BeatLeader.DataManager;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class ClanPlayer : IScoreRowContent {
        [JsonProperty("player")]
        public Player Player {
            get => HiddenPlayersCache.HidePlayerIfNeeded(originalPlayer);
            set => originalPlayer = value;
        }

        [JsonIgnore] public Player originalPlayer;

        public Score? score;

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
using BeatLeader.Components;

namespace BeatLeader.Models {
    public class ClanScore : IScoreRowContent {
        public int modifiedScore;
        public float accuracy;
        public float pp;
        public int rank;
        public string timePost;
        public Clan clan;

        #region IScoreRowContent implementation

        public bool ContainsValue(ScoreRowCellType cellType) {
            return cellType switch {
                ScoreRowCellType.Rank => true,
                ScoreRowCellType.Avatar => true,
                ScoreRowCellType.Username => true,
                ScoreRowCellType.Accuracy => true,
                ScoreRowCellType.PerformancePoints => pp > 0,
                ScoreRowCellType.Score => true,
                ScoreRowCellType.Clans => true,
                ScoreRowCellType.Time => true,
                _ => false
            };
        }

        public object? GetValue(ScoreRowCellType cellType) {
            return cellType switch {
                ScoreRowCellType.Rank => rank,
                ScoreRowCellType.Avatar => new AvatarScoreRowCell.Data(clan.avatar, null),
                ScoreRowCellType.Username => clan.name,
                ScoreRowCellType.Accuracy => accuracy,
                ScoreRowCellType.PerformancePoints => pp,
                ScoreRowCellType.Score => modifiedScore,
                ScoreRowCellType.Clans => new[] { clan },
                ScoreRowCellType.Time => timePost,
                _ => default
            };
        }

        #endregion
    }
}
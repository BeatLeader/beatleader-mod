using BeatLeader.Components;

namespace BeatLeader.Models
{
    public class Score : IScoreRowContent
    {
        public int id;
        public float accuracy;
        public float fcAccuracy;
        public int baseScore;
        public int modifiedScore;
        public string modifiers;
        public float pp;
        public float fcPp;
        public int rank;
        public int badCuts;
        public int missedNotes;
        public int bombCuts;
        public int wallsHit;
        public int pauses;
        public bool fullCombo;
        public int hmd;
        public int controller;
        public string timeSet;
        public Player player;
        public string replay;
        public string platform;

        #region IScoreRowContent implementation

        public bool ContainsValue(ScoreRowCellType cellType)
        {
            return cellType switch
            {
                ScoreRowCellType.Rank => true,
                ScoreRowCellType.Country => true,
                ScoreRowCellType.Avatar => true,
                ScoreRowCellType.Username => true,
                ScoreRowCellType.Modifiers => true,
                ScoreRowCellType.Accuracy => true,
                ScoreRowCellType.PerformancePoints => pp > 0,
                ScoreRowCellType.Score => true,
                ScoreRowCellType.Mistakes => true,
                ScoreRowCellType.Clans => true,
                ScoreRowCellType.Time => true,
                ScoreRowCellType.Pauses => true,
                _ => false
            };
        }

        public object? GetValue(ScoreRowCellType cellType)
        {
            return cellType switch
            {
                ScoreRowCellType.Rank => rank,
                ScoreRowCellType.Country => player.country,
                ScoreRowCellType.Avatar => new AvatarScoreRowCell.Data(player.avatar, player.profileSettings),
                ScoreRowCellType.Username => player.name,
                ScoreRowCellType.Modifiers => modifiers,
                ScoreRowCellType.Accuracy => accuracy,
                ScoreRowCellType.PerformancePoints => pp,
                ScoreRowCellType.Score => modifiedScore,
                ScoreRowCellType.Mistakes => missedNotes + badCuts + bombCuts + wallsHit,
                ScoreRowCellType.Clans => player.clans,
                ScoreRowCellType.Time => timeSet,
                ScoreRowCellType.Pauses => pauses,
                _ => default
            };
        }

        #endregion
    }
}
﻿using BeatLeader.Components;
using BeatLeader.DataManager;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    public class Score : IScoreRowContent {
        [JsonProperty("player")]
        public Player Player {
            get => HiddenPlayersCache.HidePlayerIfNeeded(originalPlayer);
            set => originalPlayer = value;
        }

        [JsonIgnore] public Player originalPlayer;

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
        public string? headsetName;
        public string? controllerName;
        public string timeSet;
        public string replay;
        public string platform;

        public int TotalMistakes => missedNotes + badCuts + bombCuts + wallsHit;

        #region IScoreRowContent implementation

        public bool ContainsValue(ScoreRowCellType cellType) {
            return cellType switch {
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

        public object? GetValue(ScoreRowCellType cellType) {
            return cellType switch {
                ScoreRowCellType.Rank => rank,
                ScoreRowCellType.Country => Player.country,
                ScoreRowCellType.Avatar => new AvatarScoreRowCell.Data(Player.avatar, Player.profileSettings),
                ScoreRowCellType.Username => Player.name,
                ScoreRowCellType.Modifiers => modifiers,
                ScoreRowCellType.Accuracy => accuracy,
                ScoreRowCellType.PerformancePoints => pp,
                ScoreRowCellType.Score => modifiedScore,
                ScoreRowCellType.Mistakes => TotalMistakes,
                ScoreRowCellType.Clans => Player.clans,
                ScoreRowCellType.Time => timeSet,
                ScoreRowCellType.Pauses => pauses,
                _ => default
            };
        }

        #endregion
    }
}
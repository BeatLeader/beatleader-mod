using BeatLeader.Components;
using BeatLeader.DataManager;
using Newtonsoft.Json;
using System;

namespace BeatLeader.Models {
    public class Score : IScoreRowContent {
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

        public Player ActualPlayer => _originalPlayer;

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
        private Player _originalPlayer;
        public string replay;
        public string platform;

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
                ScoreRowCellType.Mistakes => missedNotes + badCuts + bombCuts + wallsHit,
                ScoreRowCellType.Clans => Player.clans,
                ScoreRowCellType.Time => timeSet,
                ScoreRowCellType.Pauses => pauses,
                _ => default
            };
        }

        #endregion

        // important for proper work with hash sets
        public override int GetHashCode() {
            return id;
        }

        public override bool Equals(object? obj) {
            return obj is Score score && score.id == id;
        }
    }
}
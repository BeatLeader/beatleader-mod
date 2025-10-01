using System;
using BeatLeader.Models;
using Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class BeatmapReplayFilter : ITableFilter<IReplayHeader> {
        public event Action? FilterUpdatedEvent;

        public BeatmapLevelWithKey DifficultyBeatmap {
            get => _beatmap;
            set {
                _beatmap = value;
                FilterUpdatedEvent?.Invoke();
            }
        }

        private BeatmapLevelWithKey _beatmap;
        
        public bool Matches(IReplayHeader header) {
            var info = header.ReplayInfo;
            //in case beatmap is not selected showing no replays
            if (!_beatmap.HasValue) return false;

            var level = _beatmap.Level.levelID;
            var characteristic = _beatmap.Key.beatmapCharacteristic.serializedName;
            var diff = _beatmap.Key.difficulty;

            var hashMatches = level is null || level.Replace("custom_level_", "") == info.SongHash;
            var diffMatches = info.SongDifficulty == diff.ToString();
            var characteristicMatches = info.SongMode == characteristic;
            return hashMatches && diffMatches && characteristicMatches;
        }
    }
}
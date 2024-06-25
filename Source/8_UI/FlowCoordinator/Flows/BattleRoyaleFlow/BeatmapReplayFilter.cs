using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class BeatmapReplayFilter : IListFilter<IReplayHeaderBase> {
        public event Action? FilterUpdatedEvent;

        public IDifficultyBeatmap? DifficultyBeatmap {
            get => _difficultyBeatmap;
            set {
                _difficultyBeatmap = value;
                FilterUpdatedEvent?.Invoke();
            }
        }

        private IDifficultyBeatmap? _difficultyBeatmap;
        
        public bool Matches(IReplayHeaderBase header) {
            var info = header.ReplayInfo;
            //in case beatmap is not selected showing no replays
            if (_difficultyBeatmap == null) return false;

            var level = _difficultyBeatmap.level.levelID;
            var characteristic = _difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var diff = _difficultyBeatmap.difficulty;

            var hashMatches = level is null || level.Replace("custom_level_", "") == info.SongHash;
            var diffMatches = info.SongDifficulty == diff.ToString();
            var characteristicMatches = info.SongMode == characteristic;
            return hashMatches && diffMatches && characteristicMatches;
        }
    }
}
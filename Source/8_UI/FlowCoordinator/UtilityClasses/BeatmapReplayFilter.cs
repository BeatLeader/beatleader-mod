using System;
using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;

namespace BeatLeader.UI.Hub {
    internal class BeatmapReplayFilter : IReplayFilter {
        public event Action? FilterUpdatedEvent;

        public IBeatmapReplayFilterData? BeatmapFilterData {
            get => _beatmapFilterData;
            set {
                if (_beatmapFilterData is not null) {
                    _beatmapFilterData.DataUpdatedEvent -= HandleDataUpdated;
                }
                _beatmapFilterData = value;
                if (_beatmapFilterData is not null) {
                    _beatmapFilterData.DataUpdatedEvent += HandleDataUpdated;
                }
                Refresh();
            }
        }
        public bool filterOffAllWhenDataNotFull;

        private IBeatmapReplayFilterData? _beatmapFilterData;
        
        public bool MatchesFilter(IReplayInfo? info) {
            if (info is null || BeatmapFilterData is null) return false;
            //in case beatmap is not selected showing no replays
            if (filterOffAllWhenDataNotFull && !ValidateFilterData()) return false;

            var level = BeatmapFilterData.BeatmapLevel?.levelID;
            var characteristic = BeatmapFilterData.BeatmapCharacteristic?.serializedName;
            var diff = BeatmapFilterData.BeatmapDifficulty;

            var hashMatches = level is null || level.Replace("custom_level_", "") == info.SongHash;
            var diffMatches = info.SongDifficulty == diff.ToString();
            var characteristicMatches = info.SongMode == characteristic;
            return hashMatches && diffMatches && characteristicMatches;
        }
        
        private bool ValidateFilterData() {
            return BeatmapFilterData is not null
                && BeatmapFilterData.BeatmapLevel is not null
                && BeatmapFilterData.BeatmapCharacteristic is not null
                && BeatmapFilterData.BeatmapDifficulty is not null;
        }

        private void Refresh() {
            FilterUpdatedEvent?.Invoke();
        }

        private void HandleDataUpdated() {
            Refresh();
        }
    }
}
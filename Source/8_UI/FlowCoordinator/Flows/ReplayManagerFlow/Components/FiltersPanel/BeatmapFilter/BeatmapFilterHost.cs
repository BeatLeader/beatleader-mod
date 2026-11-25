using System.Collections.Generic;
using BeatLeader.Models;
using HMUI;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterHost {
        public BeatmapFilterHost() {
            _beatmapFilter = new() {
                OnBeatmapSelected = HandleBeatmapSelected
            };

            _characteristicFilter = new(_beatmapFilter);
            _difficultyFilter = new(_beatmapFilter, _characteristicFilter);

            _characteristicFilter.OnCharacteristicSelected = x => _difficultyFilter.BeatmapCharacteristic = x;

            Filters = new IPanelListFilter<IReplayHeader>[] {
                _beatmapFilter,
                _characteristicFilter,
                _difficultyFilter
            };
        }

        public IEnumerable<IPanelListFilter<IReplayHeader>> Filters { get; }

        private readonly BeatmapFilterPanel _beatmapFilter;
        private readonly CharacteristicFilterPanel _characteristicFilter;
        private readonly DifficultyFilterPanel _difficultyFilter;

        public void Setup(FlowCoordinator flowCoordinator, LevelSelectionFlowCoordinator selectionFlowCoordinator) {
            _beatmapFilter.Setup(flowCoordinator, selectionFlowCoordinator);
        }

        private void HandleBeatmapSelected(BeatmapLevelWithKey beatmap) {
            var level = beatmap.HasValue ? beatmap.Level : null;

            // Difficulties first, so they forget the previous characteristic before the new one is chosen
            _difficultyFilter.BeatmapLevel = level;
            _characteristicFilter.BeatmapLevel = level;
        }
    }
}
using System.Collections.Generic;
using BeatLeader.Models;
using HMUI;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterHost {
        public BeatmapFilterHost() {
            _beatmapFilter = new();
            _characteristicFilter = new(_beatmapFilter);
            _difficultyFilter = new(_beatmapFilter, _characteristicFilter);
            Filters = new IPanelListFilter<IReplayHeader>[] {
                _beatmapFilter,
                _characteristicFilter,
                _difficultyFilter
            };
            Init();
        }

        public IEnumerable<IPanelListFilter<IReplayHeader>> Filters { get; }

        private readonly BeatmapFilterPanel _beatmapFilter;
        private readonly CharacteristicFilterPanel _characteristicFilter;
        private readonly DifficultyFilterPanel _difficultyFilter;

        public void Setup(FlowCoordinator flowCoordinator, LevelSelectionFlowCoordinator selectionFlowCoordinator) {
            _beatmapFilter.Setup(flowCoordinator, selectionFlowCoordinator);
        }

        private void Init() {
            _beatmapFilter.BeatmapSelectedEvent += HandleBeatmapSelected;
            _characteristicFilter.CharacteristicSelectedEvent += _difficultyFilter.SetCharacteristic;
        }

        private void HandleBeatmapSelected(BeatmapLevelWithKey beatmap) {
            _characteristicFilter.SetBeatmapLevel(beatmap.Level);
            _difficultyFilter.SetBeatmapLevel(beatmap.Level);
        }
    }
}
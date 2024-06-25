using System.Collections.Generic;
using BeatLeader.Models;
using HMUI;

namespace BeatLeader.UI.Hub {
    internal class BeatmapFilterHost {
        public BeatmapFilterHost() {
            _beatmapFilter = new();
            _characteristicFilter = new(_beatmapFilter);
            _difficultyFilter = new(_beatmapFilter, _characteristicFilter);
            Filters = new IPanelListFilter<IReplayHeaderBase>[] {
                _beatmapFilter,
                _characteristicFilter,
                _difficultyFilter
            };
            Init();
        }

        public IEnumerable<IPanelListFilter<IReplayHeaderBase>> Filters { get; }

        private readonly BeatmapFilterPanel _beatmapFilter;
        private readonly CharacteristicFilterPanel _characteristicFilter;
        private readonly DifficultyFilterPanel _difficultyFilter;

        public void Setup(ViewController viewController) {
            _beatmapFilter.Setup(viewController);
        }

        private void Init() {
            _beatmapFilter.BeatmapSelectedEvent += _characteristicFilter.SetBeatmapLevel;
            _beatmapFilter.BeatmapSelectedEvent += _difficultyFilter.SetBeatmapLevel;
            _characteristicFilter.CharacteristicSelectedEvent += _difficultyFilter.SetCharacteristicCharacteristic;
        }
    }
}
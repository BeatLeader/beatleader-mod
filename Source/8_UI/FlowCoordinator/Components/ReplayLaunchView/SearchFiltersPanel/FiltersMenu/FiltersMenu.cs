using System;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class FiltersMenu : ReeUIComponentV2 {
        #region FiltersData

        public class FiltersData {
            public FiltersData(
                IPreviewBeatmapLevel? previewBeatmapLevel,
                bool overrideBeatmap,
                BeatmapDifficulty? beatmapDifficulty,
                BeatmapCharacteristicSO? beatmapCharacteristic
            ) {
                this.previewBeatmapLevel = previewBeatmapLevel;
                this.overrideBeatmap = overrideBeatmap;
                this.beatmapDifficulty = beatmapDifficulty;
                this.beatmapCharacteristic = beatmapCharacteristic;
            }

            public readonly IPreviewBeatmapLevel? previewBeatmapLevel;
            public readonly bool overrideBeatmap;
            public readonly BeatmapDifficulty? beatmapDifficulty;
            public readonly BeatmapCharacteristicSO? beatmapCharacteristic;
        }

        private IPreviewBeatmapLevel? _previewBeatmapLevel;
        private BeatmapCharacteristicSO? _beatmapCharacteristic;
        private BeatmapDifficulty _beatmapDifficulty;

        private void RefreshFiltersData() {
            FiltersChangedEvent?.Invoke(new(
                _beatmapFilter ? _previewBeatmapLevel : null, _beatmapFilter,
                _beatmapDifficultyFilter ? _beatmapDifficulty : null,
                _beatmapCharacteristicFilter ? _beatmapCharacteristic : null));
        }

        #endregion

        #region Events

        public event Action<FiltersData>? FiltersChangedEvent;

        #endregion

        #region Filters

        [UIValue("beatmap-filter"), UsedImplicitly]
        private bool BeatmapFilter {
            get => _beatmapFilter;
            set {
                _beatmapFilter = value;
                _beatmapSelector.SetActive(value);
                BeatmapCharacteristicFilter = value && BeatmapCharacteristicFilter;
                BeatmapCharacteristicInteractable = value;
                RefreshFiltersData();
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-characteristic"), UsedImplicitly]
        private bool BeatmapCharacteristicFilter {
            get => _beatmapCharacteristicFilter;
            set {
                _beatmapCharacteristicFilter = value;
                _beatmapCharacteristicPanel.SetActive(value);
                BeatmapDifficultyFilter = value && BeatmapDifficultyFilter;
                BeatmapDifficultyInteractable = value;
                RefreshFiltersData();
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-difficulty"), UsedImplicitly]
        private bool BeatmapDifficultyFilter {
            get => _beatmapDifficultyFilter;
            set {
                _beatmapDifficultyFilter = value;
                _beatmapDifficultyPanel.SetActive(value);
                RefreshFiltersData();
                NotifyPropertyChanged();
            }
        }

        private bool _beatmapDifficultyFilter;
        private bool _beatmapCharacteristicFilter;
        private bool _beatmapFilter;

        public void ResetFilters() {
            BeatmapFilter = false;
            BeatmapCharacteristicFilter = false;
            BeatmapDifficultyFilter = false;
        }

        #endregion

        #region Filters Visibility

        [UIValue("beatmap-characteristic-interactable"), UsedImplicitly]
        private bool BeatmapCharacteristicInteractable {
            get => _beatmapCharacteristicInteractable;
            set {
                _beatmapCharacteristicInteractable = value;
                //_beatmapCharacteristicPanel.SetActive(value);
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-difficulty-interactable"), UsedImplicitly]
        private bool BeatmapDifficultyInteractable {
            get => _beatmapDifficultyInteractable;
            set {
                _beatmapDifficultyInteractable = value;
                //_beatmapDifficultyPanel.SetActive(value);
                NotifyPropertyChanged();
            }
        }

        private bool _beatmapDifficultyInteractable;
        private bool _beatmapCharacteristicInteractable;

        #endregion

        #region UI Components

        [UIValue("beatmap-selector"), UsedImplicitly]
        private BeatmapSelector _beatmapSelector = null!;

        [UIValue("beatmap-characteristic-panel"), UsedImplicitly]
        private BeatmapCharacteristicPanel _beatmapCharacteristicPanel = null!;

        [UIValue("beatmap-difficulty-panel"), UsedImplicitly]
        private BeatmapDifficultyPanel _beatmapDifficultyPanel = null!;

        #endregion

        #region Init

        protected override void OnInitialize() {
            ResetFilters();
        }

        protected override void OnInstantiate() {
            _beatmapSelector = Instantiate<BeatmapSelector>(transform);
            _beatmapCharacteristicPanel = Instantiate<BeatmapCharacteristicPanel>(transform);
            _beatmapDifficultyPanel = Instantiate<BeatmapDifficultyPanel>(transform);
            _beatmapCharacteristicPanel.CharacteristicSelectedEvent += HandleCharacteristicSelectedEvent;
            _beatmapDifficultyPanel.DifficultySelectedEvent += HandleDifficultySelectedEvent;
            _beatmapSelector.BeatmapSelectedEvent += HandleBeatmapSelectedEvent;
        }

        public void Setup(
            ViewController viewController,
            FlowCoordinator flowCoordinator,
            LevelSelectionNavigationController levelSelectionNavigationController,
            LevelCollectionViewController levelCollectionViewController,
            StandardLevelDetailViewController standardLevelDetailViewController
        ) {
            _beatmapSelector.Setup(
                viewController,
                flowCoordinator,
                levelSelectionNavigationController,
                levelCollectionViewController,
                standardLevelDetailViewController);
        }

        #endregion

        #region State

        public void NotifyContainerStateChanged(bool active) {
            _beatmapSelector.NotifyBeatmapSelectorReady(active);
        }

        #endregion

        #region Callbacks

        private void HandleDifficultySelectedEvent(BeatmapDifficulty beatmapDifficulty) {
            _beatmapDifficulty = beatmapDifficulty;
            RefreshFiltersData();
        }

        private void HandleCharacteristicSelectedEvent(BeatmapCharacteristicSO characteristic) {
            _beatmapCharacteristic = characteristic;
            _beatmapDifficultyPanel.SetData(_previewBeatmapLevel?.previewDifficultyBeatmapSets
                .FirstOrDefault(x => x.beatmapCharacteristic == _beatmapCharacteristic)?
                .beatmapDifficulties.Select(x => new CustomDifficultyBeatmap(
                    null, null, x, 0, 0,
                    0, 0,
                    null, null))
                .ToArray());
            RefreshFiltersData();
        }

        private void HandleBeatmapSelectedEvent(IPreviewBeatmapLevel? level) {
            _previewBeatmapLevel = level;
            _beatmapCharacteristicPanel.SetData(
                level?.previewDifficultyBeatmapSets
                    .Select(x => new DifficultyBeatmapSet(
                        x.beatmapCharacteristic,
                        Array.Empty<IDifficultyBeatmap>()))
                    .ToArray());
            RefreshFiltersData();
        }

        #endregion
    }
}
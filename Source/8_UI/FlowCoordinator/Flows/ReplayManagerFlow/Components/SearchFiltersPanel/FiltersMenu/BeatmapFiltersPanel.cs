using System;
using System.Linq;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    //TODO: rework
    internal class BeatmapFiltersPanel : ReeUIComponentV2, IBeatmapReplayFilterData {
        #region ReplayBeatmapFilterData
        
        public IPreviewBeatmapLevel? BeatmapLevel => Enabled ? _previewBeatmapLevel : null;
        public BeatmapCharacteristicSO? BeatmapCharacteristic => _beatmapCharacteristicFilterEnabled ? _beatmapCharacteristic : null;
        public BeatmapDifficulty? BeatmapDifficulty => _beatmapDifficultyFilterEnabled ? _beatmapDifficulty : null;
        public bool Enabled { get; private set; }
        
        public event Action? DataUpdatedEvent;

        private void NotifyFilterUpdated() {
            DataUpdatedEvent?.Invoke();
        }

        #endregion

        #region Filters

        [UIValue("beatmap-filter"), UsedImplicitly]
        private bool BeatmapFilterEnabled {
            get => Enabled;
            set {
                Enabled = value;
                _beatmapSelector.SetActive(value);
                BeatmapCharacteristicFilterEnabled = value && BeatmapCharacteristicFilterEnabled;
                BeatmapCharacteristicToggleInteractable = value;
                NotifyFilterUpdated();
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-characteristic"), UsedImplicitly]
        private bool BeatmapCharacteristicFilterEnabled {
            get => _beatmapCharacteristicFilterEnabled;
            set {
                _beatmapCharacteristicFilterEnabled = value;
                _beatmapCharacteristicPanel.SetActive(value);
                BeatmapDifficultyFilterEnabled = value && BeatmapDifficultyFilterEnabled;
                BeatmapDifficultyToggleInteractable = value;
                NotifyFilterUpdated();
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-difficulty"), UsedImplicitly]
        private bool BeatmapDifficultyFilterEnabled {
            get => _beatmapDifficultyFilterEnabled;
            set {
                _beatmapDifficultyFilterEnabled = value;
                _beatmapDifficultyPanel.SetActive(value);
                NotifyFilterUpdated();
                NotifyPropertyChanged();
            }
        }

        private bool _beatmapDifficultyFilterEnabled;
        private bool _beatmapCharacteristicFilterEnabled;

        private IPreviewBeatmapLevel? _previewBeatmapLevel;
        private BeatmapCharacteristicSO? _beatmapCharacteristic;
        private BeatmapDifficulty _beatmapDifficulty;

        public void ResetFilters() {
            BeatmapFilterEnabled = false;
            BeatmapCharacteristicFilterEnabled = false;
            BeatmapDifficultyFilterEnabled = false;
        }

        #endregion

        #region Toggles

        [UIValue("beatmap-characteristic-toggle-interactable"), UsedImplicitly]
        private bool BeatmapCharacteristicToggleInteractable {
            get => _beatmapCharacteristicToggleInteractable;
            set {
                _beatmapCharacteristicToggleInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-difficulty-toggle-interactable"), UsedImplicitly]
        private bool BeatmapDifficultyToggleInteractable {
            get => _beatmapDifficultyToggleInteractable;
            set {
                _beatmapDifficultyToggleInteractable = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-toggle-enabled"), UsedImplicitly]
        private bool BeatmapToggleEnabled {
            get => _beatmapToggleEnabled;
            set {
                _beatmapToggleEnabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-characteristic-toggle-enabled"), UsedImplicitly]
        private bool BeatmapCharacteristicToggleEnabled {
            get => _beatmapCharacteristicToggleEnabled;
            set {
                _beatmapCharacteristicToggleEnabled = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("beatmap-difficulty-toggle-enabled"), UsedImplicitly]
        private bool BeatmapDifficultyToggleEnabled {
            get => _beatmapDifficultyToggleEnabled;
            set {
                _beatmapDifficultyToggleEnabled = value;
                NotifyPropertyChanged();
            }
        }
        
        public bool DisplayToggles {
            set {
                BeatmapToggleEnabled = value;
                BeatmapCharacteristicToggleEnabled = value;
                BeatmapDifficultyToggleEnabled = value;
                if (value) return;
                BeatmapFilterEnabled = true;
                BeatmapCharacteristicFilterEnabled = true;
                BeatmapDifficultyFilterEnabled = true;
            }
        }

        private bool _beatmapDifficultyToggleInteractable;
        private bool _beatmapCharacteristicToggleInteractable;
        private bool _beatmapDifficultyToggleEnabled;
        private bool _beatmapCharacteristicToggleEnabled;
        private bool _beatmapToggleEnabled;

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

        public void Setup(ViewController viewController) {
            _beatmapSelector.Setup(viewController);
        }

        protected override void OnInitialize() {
            ResetFilters();
            DisplayToggles = true;
        }

        protected override void OnInstantiate() {
            _beatmapSelector = Instantiate<BeatmapSelector>(transform);
            _beatmapCharacteristicPanel = Instantiate<BeatmapCharacteristicPanel>(transform);
            _beatmapDifficultyPanel = Instantiate<BeatmapDifficultyPanel>(transform);
            _beatmapCharacteristicPanel.CharacteristicSelectedEvent += HandleCharacteristicSelectedEvent;
            _beatmapDifficultyPanel.DifficultySelectedEvent += HandleDifficultySelectedEvent;
            _beatmapSelector.BeatmapSelectedEvent += HandleBeatmapSelectedEvent;
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
            NotifyFilterUpdated();
        }

        private void HandleCharacteristicSelectedEvent(BeatmapCharacteristicSO characteristic) {
            _beatmapCharacteristic = characteristic;
            _beatmapDifficultyPanel.SetData(
                _previewBeatmapLevel?.previewDifficultyBeatmapSets
                    .FirstOrDefault(x => x.beatmapCharacteristic == _beatmapCharacteristic)?
                    .beatmapDifficulties.Select(x => new CustomDifficultyBeatmap(
                        null,
                        null,
                        x,
                        0,
                        0,
                        0,
                        0,
                        null,
                        null))
                    .ToArray());
            NotifyFilterUpdated();
        }

        private void HandleBeatmapSelectedEvent(IPreviewBeatmapLevel? level) {
            _previewBeatmapLevel = level;
            _beatmapCharacteristicPanel.SetData(
                level?.previewDifficultyBeatmapSets
                    .Select(x => new DifficultyBeatmapSet(
                        x.beatmapCharacteristic,
                        Array.Empty<IDifficultyBeatmap>()))
                    .ToArray());
            NotifyFilterUpdated();
        }

        #endregion
    }
}
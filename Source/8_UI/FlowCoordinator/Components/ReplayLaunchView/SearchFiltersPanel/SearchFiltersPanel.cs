using System;
using System.Linq;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class SearchFiltersPanel : ReeUIComponentV2 {
        #region Events

        public event Action<string, FiltersMenu.FiltersData>? SearchDataChangedEvent;

        #endregion

        #region Search & Filters Data
        
        private FiltersMenu.FiltersData _filtersData = new(null, false, null, null);
        private string _searchPrompt = string.Empty;

        private void RefreshSearchData() {
            SearchDataChangedEvent?.Invoke(_searchPrompt, _filtersData);
        }

        #endregion

        #region UI Components

        [UIValue("search-panel"), UsedImplicitly]
        private SearchPanel _searchPanel = null!;

        [UIValue("filters-panel"), UsedImplicitly]
        private FilterPanel _filterPanel = null!;

        [UIValue("filters-menu"), UsedImplicitly]
        private FiltersMenu _filtersMenu = null!;

        [UIValue("settings-button"), UsedImplicitly]
        private ReplayerSettingsButton _replayerSettingsButton = null!;

        [UIComponent("filters-modal")]
        private readonly ModalView _modal = null!;

        #endregion

        #region Init

        public bool AllowFilters {
            get => _filterPanel.Interactable;
            set => _filterPanel.Interactable = value;
        }
        
        protected override void OnInitialize() {
            _modal.blockerClickedEvent += HandleModalClosed;
            _modal.SetField("_animateParentCanvas", false);
            _searchPanel.Placeholder = "Search Player";
        }

        protected override void OnInstantiate() {
            _searchPanel = Instantiate<SearchPanel>(transform);
            _filterPanel = Instantiate<FilterPanel>(transform);
            _filtersMenu = Instantiate<FiltersMenu>(transform);
            _replayerSettingsButton = Instantiate<ReplayerSettingsButton>(transform);
            _searchPanel.TextChangedEvent += HandleSearchPromptChanged;
            _filtersMenu.FiltersChangedEvent += HandleFiltersChanged;
            _filterPanel.FilterButtonClickedEvent += HandleFilterButtonClicked;
            _filterPanel.ResetFiltersButtonClickEvent += HandleResetFiltersButtonClicked;
        }

        public void Setup(
            IReplayManager replayManager,
            ViewController viewController,
            FlowCoordinator flowCoordinator,
            LevelSelectionNavigationController levelSelectionNavigationController,
            LevelCollectionViewController levelCollectionViewController,
            StandardLevelDetailViewController standardLevelDetailViewController
        ) {
            _replayerSettingsButton.Setup(replayManager);
            _filtersMenu.Setup(
                viewController,
                flowCoordinator,
                levelSelectionNavigationController,
                levelCollectionViewController,
                standardLevelDetailViewController);
        }

        public void NotifyContainerStateChanged() {
            _filtersMenu.NotifyContainerStateChanged(false);
        }

        #endregion

        #region Callbacks

        private void HandleModalClosed() {
            _filtersMenu.NotifyContainerStateChanged(false);
        }

        private void HandleFiltersChanged(FiltersMenu.FiltersData filters) {
            _filtersData = filters;
            _filterPanel.SetFilters(new[] {
                filters.overrideBeatmap ? "Beatmap Filter" : null,
                filters.beatmapCharacteristic is not null ? "Characteristic Filter" : null,
                filters.beatmapDifficulty is not null ? "Difficulty Filter" : null
            }.OfType<string>().ToArray());
            RefreshSearchData();
        }

        private void HandleSearchPromptChanged(string text) {
            _searchPrompt = text;
            RefreshSearchData();
        }

        private void HandleResetFiltersButtonClicked() {
            _filtersMenu.ResetFilters();
        }
        
        private void HandleFilterButtonClicked() {
            _filtersMenu.NotifyContainerStateChanged(true);
            _modal.Show(true);
        }

        #endregion
    }
}
using System;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class FilterPanel : ReeUIComponentV2 {
        #region Prefab

        private static GameObject FiltersButtonPrefab => _filtersButtonPrefab ?
            _filtersButtonPrefab! : _filtersButtonPrefab = Resources
                .FindObjectsOfTypeAll<NoTransitionsButton>().First(static
                    x => x.name == "FilterButton").gameObject;

        private static GameObject? _filtersButtonPrefab;

        #endregion

        #region Events

        public event Action? FilterButtonClickedEvent;
        public event Action? ResetFiltersButtonClickEvent;

        #endregion

        #region UI Components

        [UIComponent("container")]
        private readonly Transform _container = null!;

        private TextMeshProUGUI? _filtersText;
        private GameObject? _placeholderTextObject;
        private GameObject? _clearButtonObject;

        #endregion

        #region Init

        private bool _isInitialized;

        protected override void OnInitialize() {
            var buttonTransform = Instantiate(FiltersButtonPrefab,
                _container, false).transform;
            var button = buttonTransform
                .GetComponent<NoTransitionsButton>();
            var clearButton = buttonTransform.Find("ClearButton")
                .GetComponent<NoTransitionsButton>();
            _filtersText = buttonTransform.Find("Text")
                .GetComponent<TextMeshProUGUI>();
            _placeholderTextObject = buttonTransform.Find("PlaceholderText").gameObject;
            _clearButtonObject = buttonTransform.Find("ClearButton").gameObject;

            button.onClick.AddListener(HandleFiltersButtonClicked);
            clearButton.onClick.AddListener(HandleClearFiltersButtonClicked);
            _isInitialized = true;
            SetFilters(null);
        }

        #endregion

        #region SetFilters

        public void SetFilters(params string[]? filters) {
            if (!_isInitialized) return;
            if (_placeholderTextObject is null ||
                _filtersText is null ||
                _clearButtonObject is null) {
                throw new UninitializedComponentException();
            }
            if (filters is null || filters.Length == 0) {
                _filtersText.gameObject.SetActive(false);
                _placeholderTextObject.SetActive(true);
                _clearButtonObject.SetActive(false);
                return;
            }
            _filtersText.text = string.Join(", ", filters);
            _placeholderTextObject.SetActive(false);
            _clearButtonObject.SetActive(true);
            _filtersText.gameObject.SetActive(true);
        }

        #endregion

        #region Callbacks

        private void HandleFiltersButtonClicked() {
            FilterButtonClickedEvent?.Invoke();
        }

        private void HandleClearFiltersButtonClicked() {
            SetFilters(null);
            ResetFiltersButtonClickEvent?.Invoke();
        }

        #endregion
    }
}
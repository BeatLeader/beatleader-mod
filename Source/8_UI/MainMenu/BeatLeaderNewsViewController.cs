using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.MainMenu.NewsView.bsml")]
    internal class BeatLeaderNewsViewController : BSMLAutomaticViewController {
        #region UI Components

        [UIValue("text-news-panel"), UsedImplicitly] private TextNewsPanel _textNewsPanel = null!;

        [UIValue("map-news-panel"), UsedImplicitly] private MapNewsPanel _mapNewsPanel = null!;

        [UIValue("event-news-panel"), UsedImplicitly] private EventNewsPanel _eventNewsPanel = null!;

        [UIValue("tree-maps-panel"), UsedImplicitly] private TreeMapsPanel _treeMapsPanel = null!;

        [UIObject("tree-maps"), UsedImplicitly] 
        private GameObject _treeMaps = null!;
        [UIObject("normal-maps"), UsedImplicitly] 
        private GameObject _normalMaps = null!;

        #endregion

        #region Setup

        private void Awake() {
            _textNewsPanel = ReeUIComponentV2.Instantiate<TextNewsPanel>(transform);
            _mapNewsPanel = ReeUIComponentV2.Instantiate<MapNewsPanel>(transform);
            _eventNewsPanel = ReeUIComponentV2.Instantiate<EventNewsPanel>(transform);

            _treeMapsPanel = ReeUIComponentV2.Instantiate<TreeMapsPanel>(transform);
            _treeMapsPanel.ShouldDisplayOtherMapsEvent += ShouldDisplayOtherMapsEvent;
        }

        private void ShouldDisplayOtherMapsEvent(bool obj) {
            if (obj) {
                _treeMaps.gameObject.SetActive(false);
                _normalMaps.gameObject.SetActive(true);
            } else {
                _treeMaps.gameObject.SetActive(true);
                _normalMaps.gameObject.SetActive(false);
            }
        }

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            UpdateScreen();
            _normalMaps.gameObject.SetActive(false);
        }

        private void OnEnable() {
            if (!_initialized) return;
            UpdateScreen();
        }

        private void OnDisable() {
            RevertScreenChanges();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            gameObject.SetActive(false);
        }

        #endregion

        #region Screen Changes

        private static Vector2 TargetScreenSize => new Vector2(186, 80);

        private RectTransform _screenTransform;
        private Vector2 _originalScreenSize;
        private bool _initialized;

        private bool LazyInitializeScreen() {
            if (_initialized) return true;
            if (screen == null) return false;
            _screenTransform = screen.GetComponent<RectTransform>();
            _originalScreenSize = _screenTransform.sizeDelta;
            _initialized = true;
            return true;
        }

        private void UpdateScreen() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = TargetScreenSize;
        }

        private void RevertScreenChanges() {
            if (!LazyInitializeScreen()) return;
            _screenTransform.sizeDelta = _originalScreenSize;
        }

        #endregion
    }
}
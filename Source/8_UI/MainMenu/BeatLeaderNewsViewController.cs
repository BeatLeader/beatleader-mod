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

        #endregion

        #region Setup

        private void Awake() {
            _textNewsPanel = ReeUIComponentV2.Instantiate<TextNewsPanel>(transform);
            _mapNewsPanel = ReeUIComponentV2.Instantiate<MapNewsPanel>(transform);
            _eventNewsPanel = ReeUIComponentV2.Instantiate<EventNewsPanel>(transform);
        }

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            UpdateScreen();
        }

        private void OnEnable() {
            if (!_initialized) return;
            UpdateScreen();
        }

        private void OnDisable() {
            RevertScreenChanges();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            ReeModalSystem.CloseAll();
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
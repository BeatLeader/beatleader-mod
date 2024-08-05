using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.MainMenu.NewsView.bsml")]
    internal class BeatLeaderNewsViewController : BSMLAutomaticViewController {
        #region UI Components

        
        [UIValue("text-news-panel"), UsedImplicitly]
        private TextNewsPanel _textNewsPanel = null!;

        [UIValue("map-news-panel"), UsedImplicitly]
        private MapNewsPanel _mapNewsPanel = null!;
        
        [UIValue("event-news-panel"), UsedImplicitly]
        private EventNewsPanel _eventNewsPanel = null!;
        
        #endregion

        #region Setup

        private void Awake() {
            _textNewsPanel = ReeUIComponentV2.Instantiate<TextNewsPanel>(transform);
            _mapNewsPanel = ReeUIComponentV2.Instantiate<MapNewsPanel>(transform);
            _eventNewsPanel = ReeUIComponentV2.Instantiate<EventNewsPanel>(transform);
        }

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            //
            _textNewsPanel.Reload();
            _mapNewsPanel.Reload();
            _eventNewsPanel.Reload();
        }
        
        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            gameObject.SetActive(false);
        }

        #endregion
    }
}
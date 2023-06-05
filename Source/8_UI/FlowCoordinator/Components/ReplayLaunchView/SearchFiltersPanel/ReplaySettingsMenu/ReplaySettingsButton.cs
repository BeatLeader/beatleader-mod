using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class SettingsButton : ReeUIComponentV2 {
        #region UI Components

        [UIValue("button"), UsedImplicitly]
        private HeaderButton _button = null!;

        [UIValue("settings-menu"), UsedImplicitly]
        private SettingsMenu _settingsMenu = null!;

        [UIComponent("settings-modal")]
        private readonly ModalView _settingsModal = null!;
        
        #endregion

        #region Init

        public void Setup(IReplayManager replayManager) {
            _settingsMenu.Setup(replayManager);
        }
        
        protected override void OnInstantiate() {
            _settingsMenu = Instantiate<SettingsMenu>(transform);
            _button = Instantiate<HeaderButton>(transform);
            _button.Setup(BundleLoader.SettingsIcon);
            _button.OnClick += HandleButtonClicked;
            _settingsMenu.DeleteAllButtonClickedEvent += HandleDeleteAllButtonClicked;
        }

        #endregion

        #region Callbacks

        private void HandleDeleteAllButtonClicked() {
            _settingsModal.Hide(true);
        }

        private void HandleButtonClicked() {
            _settingsModal.Show(true, true);
        }

        #endregion
    }
}
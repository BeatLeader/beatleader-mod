using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ReplayerSettingsButton : ReeUIComponentV2 {
        #region UI Components

        [UIValue("button"), UsedImplicitly]
        private HeaderButton _button = null!;

        [UIValue("settings-menu"), UsedImplicitly]
        private ReplayerSettingsMenu _replayerSettingsMenu = null!;

        [UIComponent("settings-modal")]
        private readonly ModalView _settingsModal = null!;
        
        #endregion

        #region Init

        public void Setup(IReplayManager replayManager) {
            _replayerSettingsMenu.Setup(replayManager);
        }
        
        protected override void OnInstantiate() {
            _replayerSettingsMenu = Instantiate<ReplayerSettingsMenu>(transform);
            _button = Instantiate<HeaderButton>(transform);
            _button.Setup(BundleLoader.ReplayerSettingsIcon);
            _button.Size = 5f;
            _button.OnClick += HandleButtonClicked;
            _replayerSettingsMenu.DeleteAllButtonClickedEvent += HandleDeleteAllButtonClicked;
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
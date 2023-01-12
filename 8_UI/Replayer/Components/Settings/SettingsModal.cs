using BeatSaberMarkupLanguage.Attributes;
using HMUI;

namespace BeatLeader.Components {
    internal class SettingsModal : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("settings-modal")] private readonly ModalView _settingsModal;

        [UIValue("settings-container")] private SettingsContainer _settingsContainer;

        #endregion

        #region Setup

        public void Setup(ContentView contentView) {
            _settingsContainer.Setup(contentView);
        }

        protected override void OnInstantiate() {
            _settingsContainer = Instantiate<SettingsContainer>(transform);
        }

        protected override void OnInitialize() {
            _settingsModal.blockerClickedEvent += _settingsContainer.NotifyModalHidden;
        }

        #endregion

        #region Show & Hide

        public void ShowModal() {
            _settingsModal.Show(true);
        }

        public void HideModal() {
            _settingsModal.Hide(true);
        }

        public void HideModalImmediate() {
            _settingsModal.Hide(false);
        }

        #endregion
    }
}

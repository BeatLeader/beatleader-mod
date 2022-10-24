using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;

namespace BeatLeader.Components {
    internal class SettingsModal : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("settings-modal")] private readonly ModalView _settingsModal;

        [UIValue("settings-container")] private SettingsContainer _settingsContainer;

        #endregion

        #region Setup

        public void Setup(
            IBeatmapTimeController timeController,
            SongSpeedData speedData,
            ReplayerCameraController cameraController,
            VRControllersProvider controllersProvider,
            ReplayWatermark watermark,
            LayoutEditor layoutEditor,
            ReplayLaunchData launchData) {
            _settingsContainer.Setup(timeController, speedData, 
                cameraController, controllersProvider, watermark, layoutEditor, launchData);
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

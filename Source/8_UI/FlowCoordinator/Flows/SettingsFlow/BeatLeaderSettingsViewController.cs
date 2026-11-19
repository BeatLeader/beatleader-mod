using HMUI;
using Reactive;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderSettingsViewController : ViewController {
        #region Injection

        [Inject] private readonly MenuTransitionsHelper _menuTransitionsHelper = null!;
        [Inject] private readonly BeatLeaderHubTheme _hubTheme = null!;

        private BeatLeaderSettingsView _settingsView = null!;

        #endregion

        #region Construct

        private void Awake() {
            _settingsView = new BeatLeaderSettingsView();
            _settingsView.Use(transform);
            _settingsView.Setup(_hubTheme, _menuTransitionsHelper);
        }

        #endregion
    }
}
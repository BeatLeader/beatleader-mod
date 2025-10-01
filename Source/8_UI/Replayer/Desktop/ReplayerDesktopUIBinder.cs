using BeatLeader.Models;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Replayer.Desktop {
    internal class ReplayerDesktopUIBinder : ReplayerUIBinder {
        [Inject] private readonly ReplayerDesktopScreenSystem _screenSystem = null!;
        [Inject] private readonly ReplayerDesktopUIRenderer _renderer = null!;
        [Inject] private readonly ReplayerDesktopViewController _viewController = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        protected override void SetUIEnabled(bool uiEnabled) {
            _screenSystem.gameObject.SetActive(uiEnabled);
        }

        protected override void SetupUI() {
            if (_launchData.Settings.UISettings.ShowUIOnPause) {
                _screenSystem.ShowImmediate();
            }
            _screenSystem.Screen.SetRootViewController(_viewController, ViewController.AnimationType.None);
            _screenSystem.SetRenderCamera(_renderer.RenderCamera);
        }
    }
}
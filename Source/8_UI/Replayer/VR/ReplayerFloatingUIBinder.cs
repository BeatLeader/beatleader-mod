using HMUI;
using Zenject;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingUIBinder : ReplayerUIBinder {
        [Inject] private readonly ReplayerFloatingViewController _replayerFloatingViewController = null!;
        
        protected override void SetUIEnabled(bool uiEnabled) {
            gameObject.SetActive(uiEnabled);
        }

        protected override void SetupUI() {
            var floating = gameObject.AddComponent<FloatingScreen>();
            _replayerFloatingViewController.Setup(floating);
            floating.SetRootViewController(_replayerFloatingViewController, ViewController.AnimationType.None);
        }
    }
}
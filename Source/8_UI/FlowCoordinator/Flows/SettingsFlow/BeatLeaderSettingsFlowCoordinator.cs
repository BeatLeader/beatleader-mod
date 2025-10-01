using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderSettingsFlowCoordinator : FlowCoordinator {
        [Inject] private readonly BeatLeaderSettingsViewController _settingsViewController = null!;
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle("Settings");
            ProvideInitialViewControllers(_settingsViewController);
        }

        public override void BackButtonWasPressed(ViewController viewController) {
            _beatLeaderHubFlowCoordinator.DismissFlowCoordinator(
                this,
                animationDirection: ViewController.AnimationDirection.Vertical
            );
        }
    }
}
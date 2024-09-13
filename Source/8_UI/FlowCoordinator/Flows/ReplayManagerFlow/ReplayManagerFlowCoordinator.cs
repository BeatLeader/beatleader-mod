using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class ReplayManagerFlowCoordinator : FlowCoordinator {
        [Inject] private readonly ReplayManagerViewController _replayManagerViewController = null!;
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle("Replay Manager");
            ProvideInitialViewControllers(_replayManagerViewController);
        }

        public override void BackButtonWasPressed(ViewController viewController) {
            _beatLeaderHubFlowCoordinator.DismissFlowCoordinator(
                this, animationDirection: ViewController.AnimationDirection.Vertical
            );
        }
    }
}
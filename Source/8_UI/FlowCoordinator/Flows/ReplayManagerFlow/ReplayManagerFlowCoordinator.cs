using BeatLeader.Models;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class ReplayManagerFlowCoordinator : FlowCoordinator {
        [Inject] private readonly ReplayManagerViewController _replayManagerViewController = null!;

        public void NavigateToReplay(IReplayHeader header) {
            _replayManagerViewController.NavigateToReplay(header);
        }
        
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle("Replay Manager");
            ProvideInitialViewControllers(_replayManagerViewController);
        }

        public override void BackButtonWasPressed(ViewController viewController) {
            _parentFlowCoordinator.DismissFlowCoordinator(
                this, animationDirection: ViewController.AnimationDirection.Vertical
            );
        }
    }
}
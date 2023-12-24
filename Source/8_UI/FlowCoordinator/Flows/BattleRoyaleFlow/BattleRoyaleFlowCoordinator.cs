using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleFlowCoordinator : FlowCoordinator {
        [Inject] private readonly BattleRoyaleOpponentsViewController _opponentsViewController = null!;
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle("Battle Royale");
            ProvideInitialViewControllers(_opponentsViewController);
        }

        protected override void BackButtonWasPressed(ViewController viewController) {
            _beatLeaderHubFlowCoordinator.DismissFlowCoordinator(
                this, animationDirection: ViewController.AnimationDirection.Vertical
            );
        }
    }
}
using BeatLeader.Components;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.UI.Hub {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.HubView.HubView.bsml")]
    internal class BeatLeaderHubViewController : BSMLAutomaticViewController {
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;
        [Inject] private readonly ReplayManagerFlowCoordinator _replayManagerFlowCoordinator = null!;
        [Inject] private readonly BattleRoyaleFlowCoordinator _battleRoyaleFlowCoordinator = null!;

        [UIComponent("mini-profile"), UsedImplicitly]
        private QuickMiniProfile _quickMiniProfile = null!;

        [UIAction("#post-parse"), UsedImplicitly]
        private async void OnInitialize() {
            _quickMiniProfile.SetPlayer(null);
            //waiting for the profile load
            await ProfileManager.WaitUntilProfileLoad();
            var profile = ProfileManager.Profile ?? Player.GuestPlayer;
            _quickMiniProfile.SetPlayer(profile);
        }

        [UIAction("replay-manager-button-click"), UsedImplicitly]
        private void HandleReplayManagerButtonClicked(bool state) {
            _beatLeaderHubFlowCoordinator.PresentFlowCoordinator(
                _replayManagerFlowCoordinator,
                animationDirection: AnimationDirection.Vertical
            );
        }

        [UIAction("battle-royale-button-click"), UsedImplicitly]
        private void HandleBattleRoyaleButtonClicked(bool state) {
            _beatLeaderHubFlowCoordinator.PresentFlowCoordinator(
                _battleRoyaleFlowCoordinator,
                animationDirection: AnimationDirection.Vertical
            );
        }
    }
}
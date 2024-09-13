using System;
using BeatLeader.Manager;
using BeatLeader.UI.Hub;
using BeatLeader.ViewControllers;
using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace BeatLeader {
    internal class BeatLeaderHubFlowCoordinator : FlowCoordinator {
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly SoloFreePlayFlowCoordinator _soloFlowCoordinator = null!;
        [Inject] private readonly BeatLeaderHubMainViewController _hubMainViewController = null!;
        
        private FlowCoordinator? _parentCoordinator;

        #region Init & Dispose

        private void Awake() {
            LeaderboardEvents.MenuButtonWasPressedEvent += PresentFromLeaderboard;
        }

        private void OnDestroy() {
            LeaderboardEvents.MenuButtonWasPressedEvent -= PresentFromLeaderboard;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            FlowCoordinatorPresentedEvent?.Invoke();
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle(Plugin.PluginId);
            ProvideInitialViewControllers(_hubMainViewController);
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            FlowCoordinatorDismissedEvent?.Invoke();
        }

        public override void BackButtonWasPressed(ViewController topController) {
            Dismiss();
        }

        #endregion

        #region Present & Dismiss

        public event Action? FlowCoordinatorPresentedEvent;
        public event Action? FlowCoordinatorDismissedEvent;
        
        private void PresentFromLeaderboard() {
            Present(true);
        }

        public void Present(bool fromLeaderboard) {
            _parentCoordinator = fromLeaderboard ? _soloFlowCoordinator : _mainFlowCoordinator;
            _parentCoordinator.PresentFlowCoordinator(this);
        }

        public void Dismiss() {
            _parentCoordinator?.DismissFlowCoordinator(this);
            _parentCoordinator = null;
        }

        #endregion
    }
}
using System;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class PlayerProfileFlowCoordinator : FlowCoordinator {
        #region Open

        private static event Action<string>? OpenRequestedEvent;

        public static void Open(string playerId) {
            OpenRequestedEvent?.Invoke(playerId);
        }

        #endregion

        #region Init & Dispose

        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly PlayerProfileContext _context = null!;
        [Inject] private readonly PlayerProfileViewController _viewController = null!;

        private FlowCoordinator? _parentCoordinator;

        private void Awake() {
            OpenRequestedEvent += HandleOpenRequested;
        }

        private void OnDestroy() {
            OpenRequestedEvent -= HandleOpenRequested;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle("Player Profile");
            ProvideInitialViewControllers(_viewController);
        }

        public override void BackButtonWasPressed(ViewController topViewController) {
            _parentCoordinator?.DismissFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
            _parentCoordinator = null;
        }

        #endregion

        #region Callbacks

        private void HandleOpenRequested(string playerId) {
            _context.Load(playerId);

            if (isActivated) {
                return;
            }

            _parentCoordinator = _mainFlowCoordinator.YoungestChildFlowCoordinatorOrSelf();
            _parentCoordinator.PresentFlowCoordinator(this, animationDirection: ViewController.AnimationDirection.Vertical);
        }

        #endregion
    }
}

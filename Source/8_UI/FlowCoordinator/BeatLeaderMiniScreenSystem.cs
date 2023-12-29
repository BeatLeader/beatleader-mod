using BeatSaberMarkupLanguage;
using HMUI;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderMiniScreenSystem : MonoBehaviour {
        #region Injection

        [Inject] private readonly HierarchyManager _originalHierarchyManager = null!;
        [Inject] private readonly DiContainer _diContainer = null!;

        #endregion

        #region RootFlowCoordinator

        private class RootFlowCoordinator : FlowCoordinator {
            protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
                if (!firstActivation) return;
                var emptyViewController = BeatSaberUI.CreateViewController<ViewController>();
                ProvideInitialViewControllers(
                    emptyViewController,
                    topScreenViewController: null
                );
            }
        }

        #endregion

        #region Setup

        public FlowCoordinator FlowCoordinator { get; private set; } = null!;

        private HierarchyManager _hierarchyManager = null!;

        private void Awake() {
            SetupScreenSystem();
            FlowCoordinator = BeatSaberUI.CreateFlowCoordinator<RootFlowCoordinator>();
        }

        private void Start() {
            _hierarchyManager.StartWithFlowCoordinator(FlowCoordinator);
        }

        private void SetupScreenSystem() {
            _hierarchyManager = Instantiate(_originalHierarchyManager, transform, false);
            _diContainer.InjectGameObject(_hierarchyManager.gameObject);
            transform.localScale = Vector3.one * 0.5f;
        }

        #endregion
    }
}
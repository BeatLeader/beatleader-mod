using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    [HarmonyPatch]
    internal class BeatLeaderMiniScreenSystem : MonoBehaviour {
        #region Injection

        [Inject] private readonly HierarchyManager _originalHierarchyManager = null!;
        [Inject] private readonly DiContainer _diContainer = null!;

        #endregion

        #region RootFlowCoordinator

        [HarmonyPatch]
        private class RootFlowCoordinator : FlowCoordinator {
            #region Patch

            [HarmonyPatch(typeof(FlowCoordinator), "PresentFlowCoordinator"), HarmonyPostfix]
            private static void PresentPostfix(FlowCoordinator __instance) {
                if (__instance is not RootFlowCoordinator instance) return;
                UnbindKeyboard(instance._originalScreenSystem!);
                BindKeyboard(instance._screenSystem!);
            }

            [HarmonyPatch(typeof(FlowCoordinator), "DismissFlowCoordinator"), HarmonyPostfix]
            private static void DismissPostfix(FlowCoordinator __instance) {
                if (__instance is not RootFlowCoordinator instance) return;
                UnbindKeyboard(instance._screenSystem!);
                BindKeyboard(instance._originalScreenSystem!);
            }

            #endregion

            #region Keyboard

            private static void BindKeyboard(GameObject go) {
                var manager = go.GetComponentInChildren<UIKeyboardManager>(true);
                manager.Start();
                manager.gameObject.SetActive(true);
            }

            private static void UnbindKeyboard(GameObject go) {
                var manager = go.GetComponentInChildren<UIKeyboardManager>(true);
                manager.OnDestroy();
                manager.gameObject.SetActive(false);
            }

            #endregion

            #region Setup

            private GameObject? _originalScreenSystem;
            private GameObject? _screenSystem;

            public void Setup(GameObject screenSystem, GameObject originalScreenSystem) {
                _screenSystem = screenSystem;
                _originalScreenSystem = originalScreenSystem;
            }

            private void Start() {
                if (_screenSystem == null) return;
                UnbindKeyboard(_screenSystem);
            }

            public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
                if (firstActivation) {
                    var emptyViewController = BeatSaberUI.CreateViewController<ViewController>();
                    ProvideInitialViewControllers(
                        emptyViewController,
                        topScreenViewController: null
                    );
                }
            }

            #endregion
        }

        #endregion

        #region Setup

        public FlowCoordinator FlowCoordinator { get; private set; } = null!;

        private HierarchyManager _hierarchyManager = null!;

        private void Awake() {
            SetupScreenSystem();
            var rootCoordinator = BeatSaberUI.CreateFlowCoordinator<RootFlowCoordinator>();
            rootCoordinator.Setup(gameObject, _originalHierarchyManager.gameObject);
            FlowCoordinator = rootCoordinator;
        }

        private void Start() {
            _hierarchyManager.StartWithFlowCoordinator(FlowCoordinator);
        }

        private void SetupScreenSystem() {
            _hierarchyManager = Instantiate(_originalHierarchyManager, transform, false);
            _hierarchyManager.name = "BeatLeaderScreenSystem";
            _diContainer.InjectGameObject(_hierarchyManager.gameObject);
            transform.localScale = Vector3.one * 0.5f;
        }

        #endregion
    }
}
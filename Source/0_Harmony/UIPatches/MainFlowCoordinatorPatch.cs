using BeatLeader.UI.MainMenu;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;

namespace BeatLeader {
    [HarmonyPatch]
    internal class MainFlowCoordinatorPatch {
        private static BeatLeaderNewsViewController? _viewController;

        [HarmonyPatch(typeof(MainFlowCoordinator), "DidActivate"), HarmonyPostfix]
        private static void DidActivatePostfix(MainFlowCoordinator __instance, bool addedToHierarchy) {
            if (!addedToHierarchy) return;
            //
            if (_viewController == null) {
                _viewController = BeatSaberUI.CreateViewController<BeatLeaderNewsViewController>();
            }
            __instance.ReplaceInitialViewControllers(rightScreenViewController: _viewController);
        }
    }
}
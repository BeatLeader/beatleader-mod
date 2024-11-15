using BeatLeader.UI.MainMenu;
using BeatSaberMarkupLanguage;
using HarmonyLib;
using HMUI;

namespace BeatLeader {
    [HarmonyPatch]
    internal class MainFlowCoordinatorDidActivatePatch {
        internal static BeatLeaderNewsViewController? _viewController;

        [HarmonyPatch(typeof(MainFlowCoordinator), "DidActivate"), HarmonyPostfix]
        [HarmonyAfter(new string[] { "com.monkeymanboy.BeatSaberMarkupLanguage" })]
        private static void DidActivatePostfix(MainFlowCoordinator __instance, bool addedToHierarchy) {
            if (!addedToHierarchy || !PluginConfig.NoticeboardEnabled) return;
            //
            if (_viewController == null) {
                _viewController = BeatSaberUI.CreateViewController<BeatLeaderNewsViewController>();
            }
            __instance.ReplaceInitialViewControllers(rightScreenViewController: _viewController);
        }
    }

    [HarmonyPatch]
    internal class MainFlowCoordinatorWillChangePatch {

        internal static bool changingToMain = false;
        internal static MainFlowCoordinator? mainCoordinator = null;

        [HarmonyPatch(typeof(MainFlowCoordinator), "TopViewControllerWillChange"), HarmonyPrefix]
        private static void WillChangePrefix(MainFlowCoordinator __instance, ViewController newViewController) {
            mainCoordinator = __instance;
            changingToMain = newViewController == __instance._mainMenuViewController;
        }
    }

    [HarmonyPatch]
    internal class MainFlowCoordinatorRightScreenPatch {
        [HarmonyPatch(typeof(FlowCoordinator), "SetRightScreenViewController"), HarmonyPrefix]
        private static void RightScreenPrefix(FlowCoordinator __instance, ref ViewController viewController) {
            if (__instance == MainFlowCoordinatorWillChangePatch.mainCoordinator &&
                MainFlowCoordinatorWillChangePatch.changingToMain && 
                PluginConfig.NoticeboardEnabled &&
                viewController == null &&
                MainFlowCoordinatorDidActivatePatch._viewController != null) {
                viewController = MainFlowCoordinatorDidActivatePatch._viewController;
            }
        }
    }
}
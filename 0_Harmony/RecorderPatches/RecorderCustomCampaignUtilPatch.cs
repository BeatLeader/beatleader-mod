using BeatLeader.Utils;
using HarmonyLib;
using IPA.Loader;

namespace BeatLeader {

    /*
     * A set of patches that check if the user initiated a transition to the standard game scene through interaction with the default UI elements.
     * Current patch responsible for CustomCampaign mode. This mode uses Standard scene so part of methods are already implemented.
     *
     * All transitions beyond these are forbidden and will not trigger the replay recorder.
     * Allowed flows for now are:
     *  - Press 'Play'    button on a mission select screen
     *  - Press 'Restart' button on a mission result details screen
     *
     *  This should prevent activation of the replay recorder for any unknown ways to run GameCore scene.
     */

    #region User interaction with UI buttons

    public static class RecorderCustomCampaignUtilPatch {

        private static readonly string _pluginId = "CustomCampaigns";

        public static void ApplyPatch(Harmony harmony) {
            var type = PluginManager.GetPluginFromId(_pluginId)?.Assembly?.GetType("CustomCampaigns.Managers.CustomCampaignManager");
            if (type != null) {
                var mPlayMap = AccessTools.Method(type, "PlayMap");
                var mPlayMapPrefix = SymbolExtensions.GetMethodInfo(() => PlayButton());

                harmony.Patch(mPlayMap, new HarmonyMethod(mPlayMapPrefix));


                var mRetryButton = AccessTools.Method(type, "OnRetryButtonPressed");
                var mRetryButtonPrefix = SymbolExtensions.GetMethodInfo(() => RestartButton());

                harmony.Patch(mRetryButton, new HarmonyMethod(mRetryButtonPrefix));
            }
        }

        public static void PlayButton() {
            RecorderUtils.OnActionButtonWasPressed();
        }

        public static void RestartButton() {
            RecorderUtils.OnActionButtonWasPressed();
        }
    }

    #endregion
}
using BeatLeader.ViewControllers;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader.UIPatches {
    [HarmonyPatch(typeof(ResultsViewController), "SetDataToUI")]
    internal class ResultsViewControllerPatches {
        private static ResultsScreenUI? _resultsScreen;

        // ReSharper disable once InconsistentNaming
        [UsedImplicitly]
        private static void Postfix(ResultsViewController __instance) {
            if (_resultsScreen is null) {
                _resultsScreen = ReeUIComponentV2.Instantiate<ResultsScreenUI>(__instance.transform.Find("Container/BottomPanel"));
                _resultsScreen.gameObject.SetActive(true);
            } 
            _resultsScreen.Refresh();
        }
    }
}
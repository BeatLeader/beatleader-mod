using BeatLeader.Components;
using BeatLeader.ViewControllers;
using HarmonyLib;

namespace BeatLeader.UIPatches
{
    [HarmonyPatch(typeof(ResultsViewController), "SetDataToUI")]
    internal class ResultsViewControllerPatches
    {
        private static ResultsScreenUI _resultsScreen;

        private static void Postfix(ResultsViewController __instance)
        {
            if (_resultsScreen != null) return;

            _resultsScreen = ReeUIComponentV2.Instantiate<ResultsScreenUI>(__instance.transform.Find("Container/BottomPanel"));
            _resultsScreen.gameObject.SetActive(true);
        }
    }
}

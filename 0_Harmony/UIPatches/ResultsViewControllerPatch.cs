using BeatLeader.ViewControllers;
using HarmonyLib;
using UnityEngine;

namespace BeatLeader.UIPatches
{
    [HarmonyPatch(typeof(ResultsViewController), "SetDataToUI")]
    internal class ResultsViewControllerPatches
    {
        private static ReplayButton _replayButton;

        private static void Postfix(ResultsViewController __instance)
        {
            if (_replayButton != null) return;

            _replayButton = ReeUIComponentV2.Instantiate<ReplayButton>(
                __instance.transform.Find("Container/BottomPanel"));
            _replayButton.transform.localPosition = new Vector2(-71, 10);
            _replayButton.gameObject.SetActive(true);
        }
    }
}

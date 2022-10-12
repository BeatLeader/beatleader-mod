using BeatLeader.ViewControllers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.UIPatches
{
    [HarmonyPatch(typeof(ResultsViewController), "SetDataToUI")]
    class ResultsViewControllerPatches
    {
        static ResultsScreenUI screenUI;
        static void Postfix(ref ResultsViewController __instance)
        {
            screenUI = ReeUIComponentV2.Instantiate<ResultsScreenUI>(__instance.transform);
            screenUI.gameObject.SetActive(true);
        }
    }
}

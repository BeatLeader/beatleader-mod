using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BeatLeader.UI;

[HarmonyPatch(typeof(StandardLevelDetailView), "RefreshContent")]
public static class LevelDetailRefreshablePatch
{
    private static void Postfix(StandardLevelDetailView __instance)
    {
        ReplayUI.CheckIsReplayExists(__instance.selectedDifficultyBeatmap);
    }
}

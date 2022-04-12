using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Utils;
using HarmonyLib;
using BeatLeader.UI;

[HarmonyPatch(typeof(StandardLevelDetailView), "RefreshContent")]
public static class LevelDetailUpdatedPatch
{
    private static void Postfix(StandardLevelDetailView __instance)
    {
        EventsHandler.InvokeLevelViewUpdatedEvent(__instance.selectedDifficultyBeatmap);
    }
}

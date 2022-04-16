using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using HarmonyLib;

namespace BeatLeader //удалите это после того, как будет подргузка реплеев с облака
{
    [HarmonyPatch(typeof(StandardLevelDetailView), "RefreshContent")]
    public static class LevelDetailUpdatedPatch
    {
        private static void Postfix(StandardLevelDetailView __instance)
        {
            ReplayDataManager.lastBeatmapData = __instance.selectedDifficultyBeatmap;
            ReplayMenuUI.CheckIsReplayExists(__instance.selectedDifficultyBeatmap);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.DataManager;
using HarmonyLib;

namespace BeatLeader //удалите это после того, как будет подргузка реплеев с облака
{
    [HarmonyPatch(typeof(StandardLevelDetailViewController), "ShowContent")]
    public static class LevelDetailUpdatedPatch
    {
        private static void Postfix(StandardLevelDetailViewController __instance)
        {
            ReplayMenuUI.Refresh(__instance.selectedDifficultyBeatmap, __instance.beatmapLevel);
        }
    }
}

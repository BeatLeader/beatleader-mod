using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;
using BeatLeader.Replays;
using BeatLeader.UI;
using Zenject;

namespace BeatLeader
{
    [HarmonyPatch(typeof(GameScenesManager), "PushScenes")]
    public class ScenesTransitionDataModifierPatch
    {
        private static void Prefix(ScenesTransitionSetupDataSO scenesTransitionSetupData)
        {
            if (!ReplayMenuUI.isStartedAsReplay) return;
            var replayData = ReplayMenuUI.replayData;
            for (int i = 0; i < scenesTransitionSetupData.scenes.Length; i++)
            {
                var scene = scenesTransitionSetupData.scenes[i];
                if (scene != null && scene.sceneName != "StandardGameplay" && scene.sceneName != "GameCore")
                {
                    scene.SetField("_sceneName", ReplayDataHelper.GetEnvironmentSerializedNameByEnvironmentName(replayData.info.environment));
                }
            }
        }
    }
}

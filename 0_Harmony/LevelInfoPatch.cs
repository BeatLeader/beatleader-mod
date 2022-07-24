using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using Zenject;

namespace BeatLeader
{
    //[HarmonyPatch(typeof(MenuTransitionsHelper), "StartStandardLevel")]
    internal static class LevelInfoPatch 
    {
        private static void Postfix(string gameMode, IDifficultyBeatmap difficultyBeatmap, IPreviewBeatmapLevel previewBeatmapLevel, 
            OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, 
            PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, string backButtonText, bool useTestNoteCutSoundEffects, 
            bool startPaused, Action beforeSceneSwitchCallback, Action<DiContainer> afterSceneSwitchCallback, 
            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback)
        {
            Debug.Log(gameMode);
            Debug.LogWarning(beforeSceneSwitchCallback);
            foreach (var item in beforeSceneSwitchCallback.GetInvocationList())
            {
                Debug.Log(item.Method);
            }
            Debug.LogWarning(afterSceneSwitchCallback);
            foreach (var item in afterSceneSwitchCallback.GetInvocationList())
            {
                Debug.Log(item.Method);
            }
            Debug.LogWarning(levelFinishedCallback);
            foreach (var item in levelFinishedCallback.GetInvocationList())
            {
                Debug.Log(item.Method);
            }
        }
    }

    //[HarmonyPatch(typeof(GameScenesManager), "SceneNamesFromSceneInfoArray")]
    public class ScenePatch
    {
        private static void Prefix(SceneInfo[] sceneInfos)
        {
            foreach (var sceneInfo in sceneInfos)
            {
                Debug.LogWarning(sceneInfo);
            }
        }
    }
}

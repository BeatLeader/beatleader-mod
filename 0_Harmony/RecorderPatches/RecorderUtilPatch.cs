using System;
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Utils;
using HarmonyLib;

namespace BeatLeader {

    /*
     * A set of patches that check if the user initiated a transition to the standard game scene through interaction with the default UI elements.
     * All transitions beyond these are forbidden and will not trigger the replay recorder.
     * Allowed flows for now are:
     *  - Press 'Play'    button on a level select screen
     *  - Press 'Play'    button on a practice setup screen
     *  - Press 'Restart' button on a game pause sceen
     *  - Press 'Restart' button on a level result details screen
     *  - Failing a map with the enabled 'AutoRestart' option is considered as a restart button action in a game pause screen
     *
     *  This should prevent activation of the replay recorder for any unknown ways to run GameCore scene.
     */

    #region User interaction with UI buttons

    // Play button from a level selection screen
    [HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator), "ActionButtonWasPressed")]
    public static class ActionButtonListener {

        private static void Prefix() {
            RecorderUtils.OnActionButtonWasPressed();
        }
    }

    // Play button from a practice mode setting screen
    [HarmonyPatch(typeof(PracticeViewController), "PlayButtonPressed")]
    public static class PracticePlayButtonListener {

        private static void Prefix() {
            RecorderUtils.OnActionButtonWasPressed();
        }
    }

    // Restart button from a GameCore paused screen
    [HarmonyPatch(typeof(PauseMenuManager), "RestartButtonPressed")]
    public static class PauseRestartButtonListener {

        private static void Prefix() {
            RecorderUtils.OnRestartPauseButtonWasPressed();
        }
    }

    // Restart button from a level result screen
    [HarmonyPatch(typeof(ResultsViewController), "RestartButtonPressed")]
    public static class ResultsRestartButtonListener {
        private static void Prefix() {
            RecorderUtils.OnActionButtonWasPressed();
        }
    }

    #endregion

    #region autorestart on fail

    [HarmonyPatch(typeof(StandardLevelFailedController), "LevelFailedCoroutine")]
    internal static class AutoRestartOnFailPatch {
        private static void Prefix(StandardLevelFailedController __instance) {
            var initData = (StandardLevelFailedController.InitData)typeof(StandardLevelFailedController)
                .GetField("_initData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (initData.autoRestart) {
                RecorderUtils.OnRestartPauseButtonWasPressed();
            }
        }
    }

    #endregion

    [HarmonyPatch(typeof(GameScenesManager), "ScenesTransitionCoroutine")]
    internal static class SceneTransitionPatch {
        private static void Prefix(List<string> scenesToPresent, List<string> scenesToDismiss) {
            bool menuToGame = scenesToDismiss.Contains("MainMenu") && scenesToPresent.Contains("StandardGameplay");
            bool gameToMenu = scenesToDismiss.Contains("StandardGameplay") && scenesToPresent.Contains("MainMenu");

            bool menuToMulti = scenesToDismiss.Contains("MainMenu") && scenesToPresent.Contains("MultiplayerGameplay");
            bool multiToMenu = scenesToDismiss.Contains("MultiplayerGameplay") && scenesToPresent.Contains("MainMenu");

            RecorderUtils.OnSceneTransitionStarted(menuToGame, gameToMenu, menuToMulti, multiToMenu);
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
using BeatLeader.Utils;
using HarmonyLib;
using IPA.Loader;

namespace BeatLeader {

    /*
     * A set of patches that check if a transition to the standard game scene initiated through a standard TA procedure.
     * Current patch responsible for a TournamentAssistant mod.
     *
     * All transitions beyond these are forbidden and will not trigger the replay recorder.
     * Allowed flows for now are:
     *  - Game started by the 'Coordinator' person
     *  - Game started in automatic mode
     *
     *  This should prevent activation of the replay recorder for any unknown ways to run GameCore scene.
     */

    #region Automatic control by a coordinator

    public static class RecorderTournamentAssistantUtilPatch {

        private static readonly string _pluginId = "TournamentAssistant";

        public static void ApplyPatch(Harmony harmony) {
            PatchPlaySong(harmony);
            PatchBackButton(harmony);
        }

        private static void PatchPlaySong(Harmony harmony) {
            var type = PluginManager.GetPluginFromId(_pluginId)?.Assembly?.GetType("TournamentAssistant.Utilities.SongUtils");
            if (type != null) {
                var mPlaySong = AccessTools.Method(type, "PlaySong");
                var mPlaySongPrefix = SymbolExtensions.GetMethodInfo(() => SongIsStarting());
                harmony.Patch(mPlaySong, new HarmonyMethod(mPlaySongPrefix));
            }
        }

        private static void PatchBackButton(Harmony harmony) {
            var type = PluginManager.GetPluginFromId(_pluginId)?.Assembly?.GetType("TournamentAssistant.UI.FlowCoordinators.RoomCoordinator");
            if (type != null) {
                var mBackButtonPressed = AccessTools.Method(type, "BackButtonWasPressed");
                var mBackButtonPressedPrefix = SymbolExtensions.GetMethodInfo(() => CancelButton());
                harmony.Patch(mBackButtonPressed, new HarmonyMethod(mBackButtonPressedPrefix));
            }
        }

        public static void SongIsStarting() {
            Plugin.Log.Debug($"Enabled submission for current TA upload");
            RecorderUtils.OnActionButtonWasPressed();
        }

        private static void CancelButton() {
            Plugin.Log.Debug($"Cancelled submission in TA");
            RecorderUtils.OnCancelButtonWasPressed();
        }

    }

    #endregion
}
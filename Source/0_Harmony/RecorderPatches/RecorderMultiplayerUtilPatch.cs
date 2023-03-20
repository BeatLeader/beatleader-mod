using BeatLeader.Utils;
using HarmonyLib;

namespace BeatLeader {

    /*
     * A set of patches that check if the user initiated a transition to the game scene through interaction with the default UI elements.
     * Current patch responsible for Multiplayer scene.
     *
     * All transitions beyond these are forbidden and will not trigger the replay recorder.
     * Allowed flows for now are:
     *  - Press 'Ready'   button on a lobby waiting screen
     *  - Press 'Start'   button on a lobby waiting screen
     *  - Wait            until countdown timer is done
     *
     *  - Enabling 'spectator' option would disable other events as well as replay recorder
     *
     *  This should prevent activation of the replay recorder for any unknown ways to run GameCore scene.
     */

    #region User interaction with UI buttons

    // Start/Ready button from a setup lobby scene
    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleLobbySetupViewControllerStartGameOrReady))]
    public static class StartOrReadyActionButtonPatch {

        private static void Prefix() {
            if (PlayerActiveStateChangedEventPatch.spectator) return;
            RecorderUtils.OnActionButtonWasPressed();
        }
    }

    // Cancel/Unready button from a setup lobby scene
    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleLobbySetupViewControllerCancelGameOrUnready))]
    public static class CancelOrUnreadyActionButtonPatch {

        private static void Prefix() {
            RecorderUtils.OnCancelButtonWasPressed();
        }
    }

    // Spectator toggle button changed
    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleMultiplayerSettingsPanelControllerPlayerActiveStateChanged))]
    public static class PlayerActiveStateChangedEventPatch {

        internal static bool spectator = false;

        private static void Prefix(bool isActive) {
            if (isActive) {
                RecorderUtils.OnActionButtonWasPressed();
                spectator = false;
            } else {
                RecorderUtils.OnCancelButtonWasPressed();
                spectator = true;
            }
        }
    }

    #endregion

    #region Game state changed

    // 'Countdown started' event
    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleLobbyGameStateControllerCountdownStarted))]
    public static class CountdownStartedEventPatch {

        private static void Prefix() {
            if (PlayerActiveStateChangedEventPatch.spectator) return;
            RecorderUtils.OnActionButtonWasPressed();
        }
    }

    // 'Countdown cancelled' event
    [HarmonyPatch(typeof(GameServerLobbyFlowCoordinator), nameof(GameServerLobbyFlowCoordinator.HandleLobbyGameStateControllerCountdownCancelled))]
    public static class CountdownCancelledEventPatch {

        private static void Prefix() {
            RecorderUtils.OnCancelButtonWasPressed();
        }
    }

    #endregion

    // Player leaved multiplayer lobby (pressed leave button or disconnected)
    [HarmonyPatch(typeof(MultiplayerLobbyConnectionController), nameof(MultiplayerLobbyConnectionController.LeaveLobby))]
    public static class PlayerLeavedLobbyEventPatch {

        internal static bool spectator = false;

        private static void Prefix() {
            PlayerActiveStateChangedEventPatch.spectator = false;
            RecorderUtils.OnCancelButtonWasPressed();
        }
    }
}
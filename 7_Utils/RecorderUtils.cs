namespace BeatLeader.Utils {
    internal static class RecorderUtils {

        internal static bool shouldRecord = false;
        internal static bool buffer = false;

        internal static void OnRestartPauseButtonWasPressed() {
            shouldRecord = buffer;
            buffer = false;
        }

        internal static void OnActionButtonWasPressed() {
            shouldRecord = true;
        }

        internal static void OnSceneTransitionStarted(bool menuToGame, bool gameToMenu) {
            if (menuToGame || gameToMenu) return; // only transition related to Standard gameplay
            shouldRecord = false;
        }
    }
}
using System.Linq;

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

        internal static void OnCancelButtonWasPressed() {
            shouldRecord = false;
        }

        internal static void OnSceneTransitionStarted(params bool[] transitions) {
            if (transitions.Any(x => x)) return; // only transition related to Standard or Multiplayer gameplay
            shouldRecord = false;
        }
    }
}
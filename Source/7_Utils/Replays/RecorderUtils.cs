using System.Linq;

namespace BeatLeader.Utils {
    internal static class RecorderUtils {

        internal static bool shouldRecord = false;
        internal static bool buffer = false;

        internal static void OnRestartPauseButtonWasPressed() {
            shouldRecord = buffer;
        }

        internal static void OnActionButtonWasPressed() {
            shouldRecord = true;
        }

        internal static void OnCancelButtonWasPressed() {
            shouldRecord = false;
        }
    }
}
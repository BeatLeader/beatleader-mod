using System.Diagnostics;

namespace BeatLeader {
    public static class EnvironmentUtils {
        #region OpenBrowserPage

        public static void OpenBrowserPage(string url) {
            Process.Start("explorer.exe", $"\"{url}\"");
        }

        #endregion
    }
}
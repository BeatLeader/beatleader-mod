using UnityEngine;

namespace BeatLeader {
    public static class EnvironmentUtils {
        #region OpenBrowserPage

        public static void OpenBrowserPage(string url) {
            Application.OpenURL(url);
        }

        #endregion
    }
}
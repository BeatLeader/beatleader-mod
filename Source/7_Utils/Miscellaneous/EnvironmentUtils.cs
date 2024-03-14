using System;
using System.Linq;
using UnityEngine;

namespace BeatLeader {
    public static class EnvironmentUtils {
        #region FPFC

        public static bool UsesFPFC => OverrideUsesFPFC ?? hasFpfcArg;
        
        public static bool? OverrideUsesFPFC { get; set; }
        
        private static readonly bool hasFpfcArg = Environment
            .GetCommandLineArgs()
            .Contains("fpfc");
        
        #endregion
        
        #region OpenBrowserPage

        public static void OpenBrowserPage(string url) {
            Application.OpenURL(url);
        }

        #endregion
    }
}
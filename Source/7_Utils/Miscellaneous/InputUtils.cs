using System;
using UnityEngine;
using System.Linq;

namespace BeatLeader.Utils {
    internal static class InputUtils {
        #region FPFC

        public static bool UsesFPFC => OverrideUsesFPFC ?? HasFpfcArg;

        public static bool? OverrideUsesFPFC { get; set; }

        public static readonly bool HasFpfcArg = Environment
            .GetCommandLineArgs()
            .Contains("fpfc");

        #endregion

        #region Cursor

        public static void SwitchCursor() {
            EnableCursor(!Cursor.visible);
        }

        public static void EnableCursor(bool enable) {
            Cursor.visible = enable;
            Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        }

        #endregion
    }
}
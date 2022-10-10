using System;
using UnityEngine;
using System.Linq;

namespace BeatLeader.Utils
{
    public static class InputUtils
    {
        [Flags] public enum InputType
        {
            VR = 1,
            FPFC = 2
        }

        public static bool IsInFPFC => Environment.GetCommandLineArgs().Contains("fpfc");

        public static bool MatchesCurrentInput(InputType type)
        {
            return type.HasFlag(IsInFPFC ? InputType.FPFC : InputType.VR);
        }
        public static void SwitchCursor()
        {
            EnableCursor(!Cursor.visible);
        }
        public static void EnableCursor(bool enable)
        {
            Cursor.visible = enable;
            Cursor.lockState = enable ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
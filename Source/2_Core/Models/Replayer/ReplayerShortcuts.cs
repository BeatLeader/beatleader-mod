using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerShortcuts {
        public KeyCode LayoutEditorPartialModeHotkey { get; set; }
        public KeyCode HideCursorHotkey { get; set; }
        public KeyCode PauseHotkey { get; set; }
        public KeyCode RewindForwardHotkey { get; set; }
        public KeyCode RewindBackwardHotkey { get; set; }
    }
}

using IPA.Config.Stores.Attributes;
using UnityEngine;

namespace BeatLeader.Models {
    public class ReplayerShortcuts {
        [UseConverter]
        public KeyCode LayoutEditorPartialModeHotkey { get; set; }

        [UseConverter]
        public KeyCode HideCursorHotkey { get; set; }

        [UseConverter]
        public KeyCode PauseHotkey { get; set; }

        [UseConverter]
        public KeyCode RewindForwardHotkey { get; set; }

        [UseConverter]
        public KeyCode RewindBackwardHotkey { get; set; }

        [UseConverter]
        public KeyCode LayoutEditorAntiSnapHotkey { get; set; }
    }
}

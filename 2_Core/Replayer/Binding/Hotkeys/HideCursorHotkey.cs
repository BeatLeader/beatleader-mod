using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    internal class HideCursorHotkey : GameHotkey
    {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.HideCursorHotkey;

        [Inject] private ReplayLaunchData _launchData;

        public override void OnKeyDown()
        {
            InputManager.SwitchCursor();
        }
    }
}

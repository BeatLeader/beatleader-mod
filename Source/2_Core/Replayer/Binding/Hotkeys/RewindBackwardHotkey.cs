using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class RewindBackwardHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.RewindBackwardHotkey;

        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        public override void OnKeyDown() {
            var settings = _launchData.Settings.UISettings.Controls;
            if (!ReplayControlsActions.FpfcHotkeysEnabled(settings)) return;
            ReplayControlsActions.Seek(_timeController, settings, -1);
        }
    }
}

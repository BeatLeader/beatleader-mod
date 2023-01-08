using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class RewindBackwardHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.RewindBackwardHotkey;

        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        public override void OnKeyDown() {
            _timeController.Rewind(_timeController.SongTime - 5);
        }
    }
}

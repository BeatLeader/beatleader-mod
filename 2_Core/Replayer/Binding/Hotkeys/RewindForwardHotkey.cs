using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class RewindForwardHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.RewindForwardHotkey;

        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IReplayTimeController _timeController = null!;

        public override void OnKeyDown() {
            _timeController.Rewind(_timeController.SongTime + 5);
        }
    }
}

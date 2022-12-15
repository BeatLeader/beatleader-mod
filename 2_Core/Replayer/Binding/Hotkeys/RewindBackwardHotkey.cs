using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    internal class RewindBackwardHotkey : GameHotkey
    {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.RewindBackwardHotkey;

        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly IBeatmapTimeController _beatmapTimeController;

        public override void OnKeyDown()
        {
            _beatmapTimeController.Rewind(_beatmapTimeController.SongTime - 5);
        }
    }
}

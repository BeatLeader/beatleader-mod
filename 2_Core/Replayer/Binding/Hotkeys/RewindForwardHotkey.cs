using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    internal class RewindForwardHotkey : GameHotkey
    {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.RewindForwardHotkey;

        [Inject] private ReplayLaunchData _launchData;
        [Inject] private IBeatmapTimeController _beatmapTimeController;

        public override void OnKeyDown()
        {
            _beatmapTimeController.Rewind(_beatmapTimeController.SongTime + 5);
        }
    }
}

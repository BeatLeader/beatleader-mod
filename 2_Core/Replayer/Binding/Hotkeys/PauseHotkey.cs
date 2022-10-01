using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    internal class PauseHotkey : GameHotkey
    {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.PauseHotkey;

        [Inject] private PlaybackController _playbackController;
        [Inject] private ReplayLaunchData _launchData;

        public override void OnKeyDown()
        {
            _playbackController.Pause(!_playbackController.IsPaused);
        }
    }
}

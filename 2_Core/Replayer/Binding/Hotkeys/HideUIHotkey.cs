using BeatLeader.Components;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    internal class HideUIHotkey : GameHotkey
    {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.HideUIHotkey;

        [InjectOptional] private UI2DManager _uiManager;
        [Inject] private ReplayLaunchData _launchData;

        public override void OnKeyDown()
        {
            if (_uiManager != null)
                _uiManager.ShowUI = !_uiManager.ShowUI;
        }
    }
}

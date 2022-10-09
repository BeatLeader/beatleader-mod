using BeatLeader.Models;
using BeatLeader.UI;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding
{
    internal class HideUIHotkey : GameHotkey
    {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.HideUIHotkey;

        [InjectOptional] private ReplayerUIBinder _uiBinder;
        [Inject] private ReplayLaunchData _launchData;

        public override void OnKeyDown()
        {
            if (_uiBinder != null && InputUtils.IsInFPFC)
                _uiBinder.ShowUI = !_uiBinder.ShowUI;
        }
    }
}

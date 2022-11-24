using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.ViewControllers;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class LayoutEditorHotkey : GameHotkey {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.LayoutEditorHotkey;

        [InjectOptional] private readonly Replayer2DViewController _viewController;
        [Inject] private readonly ReplayLaunchData _launchData;

        public override void OnKeyDown() {
            _viewController?.OpenLayoutEditor();
        }
    }
}

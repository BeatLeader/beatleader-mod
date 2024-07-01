using BeatLeader.Models;
using BeatLeader.ViewControllers;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class PartialDisplayModeHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.LayoutEditorPartialModeHotkey;

        [InjectOptional] private readonly Replayer2DViewController? _viewController;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override void OnKeyDown() {
            //_viewController?.SwitchLayoutEditorPartialMode();
        }
    }
}

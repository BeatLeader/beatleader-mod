using BeatLeader.Models;
using BeatLeader.UI.Replayer.Desktop;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class LayoutEditorModeHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.LayoutEditorPartialModeHotkey;

        [InjectOptional] private readonly ReplayerDesktopViewController? _viewController;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override void OnKeyDown() {
            if (_launchData.Settings.UISettings.ShowUIOnPause) return;
            _viewController?.SwitchViewMode();
        }
    }
}

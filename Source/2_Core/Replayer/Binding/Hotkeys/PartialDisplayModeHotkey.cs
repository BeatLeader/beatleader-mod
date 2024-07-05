using BeatLeader.Models;
using BeatLeader.UI.Replayer.Desktop;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class PartialDisplayModeHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.LayoutEditorPartialModeHotkey;

        [InjectOptional, UsedImplicitly] private readonly ReplayerDesktopViewController? _viewController;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public override void OnKeyDown() {
            if (_launchData.Settings.UISettings.AutoHideUI) return;
            _viewController?.SwitchPartialDisplayMode();
        }
    }
}

using BeatLeader.Models;
using BeatLeader.UI;
using BeatLeader.ViewControllers;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class LayoutEditorHotkey : GameHotkey {
        public override KeyCode Key => _launchData.ActualSettings.Shortcuts.LayoutEditorHotkey;

        [InjectOptional] private readonly Replayer2DViewController _viewController;
        [InjectOptional] private readonly ReplayerUIBinder _binder;
        [Inject] private readonly ReplayLaunchData _launchData;

        public override void OnKeyDown() {
            if (_binder != null && _binder.UIEnabled) {
                _viewController?.OpenLayoutEditor();
            }
        }
    }
}

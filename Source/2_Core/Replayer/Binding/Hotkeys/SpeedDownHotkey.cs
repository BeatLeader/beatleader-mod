using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class SpeedDownHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.SpeedDownHotkey;

        public override IEnumerable<KeyCode> Keys {
            get {
                yield return Key;
                if (Key == KeyCode.Minus) {
                    yield return KeyCode.KeypadMinus;
                }
            }
        }

        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        public override void OnKeyDown() {
            var settings = _launchData.Settings.UISettings.Controls;
            if (!ReplayControlsActions.FpfcHotkeysEnabled(settings)) return;
            ReplayControlsActions.ChangeSpeed(_timeController, settings, -1, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }
    }
}

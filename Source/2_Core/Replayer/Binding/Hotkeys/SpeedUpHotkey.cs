using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Binding {
    internal class SpeedUpHotkey : GameHotkey {
        public override KeyCode Key => _launchData.Settings.Shortcuts.SpeedUpHotkey;

        public override IEnumerable<KeyCode> Keys {
            get {
                yield return Key;
                if (Key == KeyCode.Plus) {
                    yield return KeyCode.Equals;
                    yield return KeyCode.KeypadPlus;
                }
            }
        }

        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly IBeatmapTimeController _timeController = null!;

        public override void OnKeyDown() {
            var settings = _launchData.Settings.Controls;
            if (!ReplayControlsActions.FpfcHotkeysEnabled(settings)) return;
            ReplayControlsActions.ChangeSpeed(_timeController, settings, 1, Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }
    }
}

using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    public class SpeedSlider : ReactiveComponent {
        private IBeatmapTimeController? _timeController;
        private BsSlider _speedSlider = null!;
        private bool _syncingSlider;

        public void Setup(IBeatmapTimeController timeController) {
            if (_timeController != null) {
                _timeController.SongSpeedWasChangedEvent -= HandleTimeControllerSpeedChanged;
            }
            _timeController = timeController;
            _timeController.SongSpeedWasChangedEvent += HandleTimeControllerSpeedChanged;
            HandleTimeControllerSpeedChanged(timeController.SongSpeedMultiplier);
        }

        protected override GameObject Construct() {
            return new BsSlider {
                MinValue = 1f,
                MaxValue = 200f,
                ValueFormatter = x => $"{x}%",
                ValueStep = 1f,
                OnValueChanged = HandleSliderValueChanged
            }.Bind(ref _speedSlider).InNamedRail("Playback Speed").Use();
        }

        private void HandleSliderValueChanged(float value) {
            if (_syncingSlider) return;
            _timeController?.SetSpeedMultiplier(value / 100f);
        }

        private void HandleTimeControllerSpeedChanged(float speed) {
            _syncingSlider = true;
            _speedSlider.Value = speed * 100f;
            _syncingSlider = false;
        }
    }
}
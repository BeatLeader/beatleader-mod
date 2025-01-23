using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    public class SpeedSlider : ReactiveComponent {
        private IBeatmapTimeController? _timeController;
        private Slider _speedSlider = null!;

        public void Setup(IBeatmapTimeController timeController) {
            if (_timeController != null) {
                _timeController.SongSpeedWasChangedEvent -= HandleTimeControllerSpeedChanged;
            }
            _timeController = timeController;
            _timeController.SongSpeedWasChangedEvent += HandleTimeControllerSpeedChanged;
            HandleTimeControllerSpeedChanged(timeController.SongSpeedMultiplier);
        }

        protected override GameObject Construct() {
            return new Slider {
                ValueRange = new() {
                    Start = 1f,
                    End = 200f
                },
                ValueFormatter = x => $"{x}%",
                ValueStep = 1f
            }.WithListener(
                x => x.Value,
                x => _timeController?.SetSpeedMultiplier(x / 100f)
            ).Bind(ref _speedSlider).InNamedRail("Playback Speed").Use();
        }

        private void HandleTimeControllerSpeedChanged(float speed) {
            _speedSlider.SetValueSilent(speed * 100f);
        }
    }
}
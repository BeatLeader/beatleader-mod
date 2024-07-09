using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsOtherView : ReactiveComponent {
        #region Setup

        private IBeatmapTimeController? _timeController;
        private IReplayFinishController? _finishController;

        public void Setup(
            IBeatmapTimeController timeController,
            IReplayFinishController finishController
        ) {
            if (_timeController != null) {
                _timeController.SongSpeedWasChangedEvent -= HandleTimeControllerSpeedChanged;
            }
            _timeController = timeController;
            _finishController = finishController;

            _timeController.SongSpeedWasChangedEvent += HandleTimeControllerSpeedChanged;
            _exitToggle.SetActive(finishController.ExitAutomatically, false, true);
            HandleTimeControllerSpeedChanged(timeController.SongSpeedMultiplier);
        }

        #endregion

        #region Construct

        private Slider _speedSlider = null!;
        private Toggle _exitToggle = null!;

        protected override GameObject Construct() {
            static Image CreateContainer(float gap, params ILayoutItem[] children) {
                return new Image()
                    .With(x => x.Children.AddRange(children))
                    .AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexGroup(
                        direction: FlexDirection.Column,
                        padding: 2f,
                        justifyContent: Justify.FlexStart,
                        gap: gap
                    ).AsFlexItem();
            }

            return new Image {
                Children = {
                    //speed
                    new Slider {
                        ValueRange = new() {
                            Start = 20f,
                            End = 200f
                        },
                        ValueFormatter = x => $"{x}%",
                        ValueStep = 5f
                    }.WithListener(
                        x => x.Value,
                        x => _timeController?.SetSpeedMultiplier(x / 100f)
                    ).Bind(ref _speedSlider).InNamedRail("Playback Speed"),
                    //
                    new Toggle().WithListener(
                        x => x.Active,
                        HandleToggleStateChanged
                    ).Bind(ref _exitToggle).InNamedRail("Auto-exit on finish")
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                gap: 2f,
                padding: 2f
            ).AsBackground(
                color: new(0.1f, 0.1f, 0.1f, 1f)
            ).Use();
        }

        #endregion

        #region Callbacks

        private void HandleToggleStateChanged(bool state) {
            if (_finishController == null) return;
            _finishController.ExitAutomatically = state;
        }

        private void HandleTimeControllerSpeedChanged(float speed) {
            _speedSlider.SetValueSilent(speed * 100f);
        }

        #endregion
    }
}
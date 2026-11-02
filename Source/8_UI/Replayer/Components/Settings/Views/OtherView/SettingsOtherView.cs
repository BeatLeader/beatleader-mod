using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class SettingsOtherView : ReactiveComponent {
        #region Setup

        private IReplayFinishController? _finishController;

        public void Setup(IBeatmapTimeController timeController, IReplayFinishController finishController) {
            _finishController = finishController;
            _speedSlider.Setup(timeController);
            _exitToggle.Toggled = finishController.ExitAutomatically;
        }

        #endregion

        #region Construct

        private SpeedSlider _speedSlider = null!;
        private BsToggle _exitToggle = null!;

        protected override GameObject Construct() {
            return new Background {
                Children = {
                    //speed
                    new SpeedSlider().Bind(ref _speedSlider),
                    //
                    new BsToggle {
                        OnToggleStateChanged = HandleToggleStateChanged
                    }.Bind(ref _exitToggle).InNamedRail("Auto-exit on finish")
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
            _finishController?.ExitAutomatically = state;
        }

        #endregion
    }
}
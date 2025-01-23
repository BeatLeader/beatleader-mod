using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
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
            _exitToggle.SetActive(finishController.ExitAutomatically, false, true);
        }

        #endregion

        #region Construct

        private SpeedSlider _speedSlider = null!;
        private Toggle _exitToggle = null!;

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    //speed
                    new SpeedSlider().Bind(ref _speedSlider),
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

        #endregion
    }
}
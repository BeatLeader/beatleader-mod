using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingViewController : ViewController {
        #region Injection

        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly IVirtualPlayerBodySpawner _bodySpawner = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ICameraController _cameraController = null!;
        [Inject] private readonly IReplayWatermark _watermark = null!;

        #endregion

        #region Setup

        private FloatingScreen? _floatingScreen;
        private bool _isConstructed;
        
        public void Setup(FloatingScreen floatingScreen) {
            _floatingScreen = floatingScreen;
            if (!_isConstructed) return;
            SetupInternal();
        }

        private void SetupInternal() {
            _floatingPanelControls.Setup(
                _floatingScreen!,
                _cameraController.Camera,
                _launchData.Settings.UISettings.FloatingSettings ?? throw new ArgumentException(
                    "Floating settings cannot be null when using floating view"
                )
            );
        }

        private ToolbarWithSettings _toolbar = null!;
        private ReplayerFloatingPanelControls _floatingPanelControls = null!;

        private void Awake() {
            new Dummy {
                Children = {
                    new ToolbarWithSettings()
                        .AsFlexItem(size: new() { x = 80f, y = 70f })
                        .Bind(ref _toolbar),
                    //
                    new ReplayerFloatingPanelControls()
                        .AsFlexItem(alignSelf: Align.Center)
                        .Bind(ref _floatingPanelControls)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                gap: 1f
            ).WithSizeDelta(80f, 83f /* toolbar + controls + gap */).Use(transform);
            //
            _toolbar.Setup(
                _pauseController,
                _finishController,
                _timeController,
                _playersManager,
                _cameraController,
                _bodySpawner,
                _launchData,
                null,
                _watermark,
                false
            );
            if (_floatingScreen != null) {
                SetupInternal();
            }
            _isConstructed = true;
        }

        #endregion
    }
}
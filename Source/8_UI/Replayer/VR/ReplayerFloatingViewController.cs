using System;
using BeatLeader.Models;
using HMUI;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using Zenject;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerFloatingViewController : ViewController {
        #region Injection

        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly IBodySettingsViewFactory _bodySettingsFactory = null!;
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
                _launchData.Settings.UISettings.FloatingSettings ?? throw new InvalidOperationException(
                    "Floating settings cannot be null when using floating view"
                )
            );
        }

        private ToolbarWithSettings _toolbar = null!;
        private ReplayerFloatingPanelControls _floatingPanelControls = null!;

        private void Awake() {
            new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Gap = 1f,
                    ConstrainHorizontal = false,
                    ConstrainVertical = false
                },

                Children = {
                    new ToolbarWithSettings {
                        FlexItem = {
                            Size = new() { x = 80f, y = 70f }
                        }
                    }.Bind(ref _toolbar),
                    //
                    new ReplayerFloatingPanelControls {
                        FlexItem = {
                            AlignSelf = Align.Center
                        }
                    }.Bind(ref _floatingPanelControls)
                }
            }.Use(transform);
            //
            _toolbar.Setup(
                _pauseController,
                _finishController,
                _timeController,
                _playersManager,
                _cameraController,
                _bodySettingsFactory,
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
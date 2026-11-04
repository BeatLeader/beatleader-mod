using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Replayer {
    internal class ToolbarWithSettings : ReactiveComponent, Toolbar.ISettingsPanel {
        #region Animation

        private AnimatedState<float> _animatedValue = null!;

        public bool IsPresent {
            get => _animatedValue.TargetValue > 0f;
            set {
                if (value) {
                    _settingsPanel.Enabled = true;
                }

                _animatedValue.TargetValue = value ? 1f : 0f;
            }
        }

        private void RefreshAnimation(float progress) {
            var settingsTransform = _settingsPanel.ContentTransform;
            var pos = Mathf.Lerp(-settingsTransform.rect.height, 0f, progress);
            var scale = Mathf.Lerp(0.8f, 1f, progress);

            settingsTransform.localPosition = new(0f, pos, 0f);
            settingsTransform.localScale = Vector3.one * scale;
        }

        #endregion

        #region Setup

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            ICameraController cameraController,
            IBodySettingsViewFactory bodySettingsFactory,
            ReplayLaunchData launchData,
            ILayoutEditor? layoutEditor,
            IReplayWatermark watermark,
            bool useAlternativeBlur
        ) {
            _toolbar.Setup(
                pauseController,
                finishController,
                timeController,
                playersManager,
                this,
                launchData.Settings.UISettings
            );
            _settingsPanel.Setup(
                launchData,
                timeController,
                finishController,
                cameraController,
                bodySettingsFactory,
                layoutEditor,
                _toolbar.Timeline,
                watermark,
                useAlternativeBlur
            );
        }

        #endregion

        #region Construct

        private ReplayerSettingsPanel _settingsPanel = null!;
        private Toolbar _toolbar = null!;

        protected override GameObject Construct() {
            _animatedValue = RememberAnimated(0f, 10.fact);
            _animatedValue.AddCallback(RefreshAnimation, lazy: true);

            _animatedValue.OnFinish += x => _settingsPanel.Enabled = x != 0f;

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Gap = 1f
                },

                Children = {
                    //settings container
                    new Layout {
                        FlexItem = {
                            FlexGrow = 1f
                        },

                        ContentTransform = {
                            pivot = new(0.5f, 0f)
                        },
                        Children = {
                            //settings
                            new ReplayerSettingsPanel {
                                Enabled = false,
                                ContentTransform = {
                                    pivot = new(0.5f, 0f)
                                }
                            }.WithRectExpand().Bind(ref _settingsPanel)
                        }
                    }.WithNativeComponent(out RectMask2D _),
                    //toolbar
                    new Toolbar {
                        FlexItem = {
                            Size = new() { y = 10f }
                        }
                    }.Bind(ref _toolbar)
                }
            }.Use();
        }

        #endregion
    }
}
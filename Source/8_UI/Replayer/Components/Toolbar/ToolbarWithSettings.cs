using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Replayer {
    internal class ToolbarWithSettings : ReactiveComponent, Toolbar.ISettingsPanel {
        #region Animation

        private AnimatedValue<float> _animatedValue = null!;

        public void Present() {
            _settingsPanel.Enabled = true;
            _animatedValue.Value = 1f;
        }

        public void Dismiss() {
            _animatedValue.Value = 0f;
        }

        private void RefreshAnimation(float progress) {
            var targetPos = _settingsTransform.rect.height;
            var pos = Mathf.Lerp(-targetPos, 0f, progress);
            var scale = Mathf.Lerp(0.8f, 1f, progress);

            _settingsTransform.localPosition = new(0f, pos, 0f);
            _settingsTransform.localScale = Vector3.one * scale;
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
                launchData.Settings,
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

        private RectTransform _settingsTransform = null!;
        private ReplayerSettingsPanel _settingsPanel = null!;
        private Toolbar _toolbar = null!;

        protected override GameObject Construct() {
            _animatedValue = RememberAnimated(0f, 10.fact());
            _animatedValue.ValueChangedEvent += RefreshAnimation;

            _animatedValue.OnFinish += x => _settingsPanel.Enabled = x.Value != 0f;

            return new Layout {
                Children = {
                    //settings container
                    new Layout {
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
                                }.WithRectExpand().Bind(ref _settingsTransform).Bind(ref _settingsPanel)
                            }
                        }
                        .WithNativeComponent(out RectMask2D _)
                        .AsFlexItem(flexGrow: 1f),
                    //toolbar
                    new Toolbar().AsFlexItem(size: new() { y = 10f }).Bind(ref _toolbar)
                }
            }.AsFlexGroup(direction: FlexDirection.Column, gap: 1f).Use();
        }

        #endregion
    }
}
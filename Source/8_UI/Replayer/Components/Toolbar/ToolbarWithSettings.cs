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

        private readonly ValueAnimator _valueAnimator = new() { LerpCoefficient = 15f };

        public void Present() {
            _settingsTransform.gameObject.SetActive(true);
            _valueAnimator.Push();
        }

        public void Dismiss() {
            _valueAnimator.Pull();
        }

        protected override void OnUpdate() {
            _valueAnimator.Update();
            RefreshAnimation(_valueAnimator.Progress);
        }

        private void RefreshAnimation(float progress) {
            var targetPos = _settingsTransform.rect.height;
            var pos = Mathf.Lerp(-targetPos, 0f, progress);
            var scale = Mathf.Lerp(0.8f, 1f, progress);
            _settingsTransform.localPosition = new(0f, pos, 0f);
            _settingsTransform.localScale = Vector3.one * scale;
            if (progress <= 0.01f) {
                _settingsTransform.gameObject.SetActive(false);
            }
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

        protected override void OnInitialize() {
            _valueAnimator.SetTarget(0f);
            _valueAnimator.SetProgress(0f);
        }

        #endregion
    }
}
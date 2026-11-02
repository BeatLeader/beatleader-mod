using System;
using UnityEngine;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Replayer {
    internal class Toolbar : ReactiveComponent {
        #region Setup

        public IReplayTimeline Timeline => _timeline;

        private IReplayPauseController? _pauseController;
        private IReplayFinishController? _finishController;
        private IReplayTimeController? _beatmapTimeController;
        private ISettingsPanel? _settingsPanel;

        public void Setup(
            IReplayPauseController pauseController,
            IReplayFinishController finishController,
            IReplayTimeController timeController,
            IVirtualPlayersManager playersManager,
            ISettingsPanel settingsPanel,
            ReplayerUISettings uiSettings
        ) {
            if (_pauseController != null) {
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
            }
            _pauseController = pauseController;
            _finishController = finishController;
            _beatmapTimeController = timeController;
            _settingsPanel = settingsPanel;

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _timeline.Setup(playersManager, pauseController, timeController, uiSettings);
            HandlePauseStateChanged(_pauseController.IsPaused);
        }

        protected override void OnUpdate() {
            UpdateSongTime();
        }

        protected override void OnInitialize() {
            SetSongTime(0, 0);
        }

        protected override void OnDestroy() {
            if (_pauseController != null) {
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
            }
        }

        #endregion

        #region Construct

        private Timeline _timeline = null!;
        private Label _timeText = null!;
        private Image _playButton = null!;
        private State<GraphicState> _settingsButtonState = null!;

        [StateGen]
        private static Image CreateGlowingButton(Sprite? sprite, State<GraphicState> state, Action onClick) {
            return new Image {
                Sprite = sprite,
                Material = BundleLoader.Materials.uiAdditiveGlowMaterial,
                PreserveAspect = true,
                sColor = state.MapColorSet(UIStyle.GlowingButtonColorSet)
            }.WithPointerEvents(
                onEnter: _ => state.IsHovered = true,
                onLeave: _ => state.IsHovered = false,
                onDown: _ => {
                    onClick();
                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                }
            );
        }

        protected override GameObject Construct() {
            _settingsButtonState = Remember(GraphicState.None);

            return new Layout {
                Children = {
                    new ExitButton {
                            OnClick = HandleExitButtonClicked
                        }
                        .AsFlexItem(flexGrow: 1f)
                        .InBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                        .AsFlexGroup(padding: new() { top = 1.5f, bottom = 1.5f, right = 1f })
                        .AsFlexItem(basis: 10f),
                    //toolbar
                    new Background {
                        Children = {
                            //play button
                            CreateGlowingButton(
                                null,
                                Remember(GraphicState.None),
                                HandlePlayButtonClicked
                            ).Bind(ref _playButton).AsFlexItem(
                                basis: 5f,
                                margin: new() { left = 1f, right = 1f }
                            ),
                            //timeline
                            new Timeline().Bind(ref _timeline).AsFlexItem(
                                flexGrow: 1f,
                                alignSelf: Align.Center
                            ),
                            //text
                            new Label {
                                FontSize = 3.5f,
                                Alignment = TextAlignmentOptions.Center
                            }.Bind(ref _timeText).AsFlexItem(
                                size: "auto",
                                minSize: new() { x = 12 },
                                alignSelf: Align.Center
                            ),
                            //settings button
                            CreateGlowingButton(
                                BundleLoader.SettingsIcon,
                                _settingsButtonState,
                                HandleSettingsButtonClicked
                            ).AsFlexItem(
                                basis: 6f,
                                margin: new() { left = 1f }
                            )
                        }
                    }.AsBackground(
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexGroup(
                        padding: new() { top = 2f, bottom = 2f, left = 1f, right = 1.5f },
                        gap: 1f
                    ).AsFlexItem(flexGrow: 1f)
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        #endregion

        #region SongTime

        private void UpdateSongTime() {
            var timeController = _beatmapTimeController!;
            SetSongTime(timeController.SongTime, timeController.ReplayEndTime);
        }

        private void SetSongTime(float time, float totalTime) {
            _timeText.Text = FormatUtils.FormatSongTime(time, totalTime);
        }

        #endregion

        #region PlayButton

        private static readonly Sprite playSprite = BundleLoader.PlayIcon;
        private static readonly Sprite pauseSprite = BundleLoader.PauseIcon;

        private void RefreshPlayButton(bool paused) {
            _playButton.Sprite = paused ? playSprite : pauseSprite;
        }

        #endregion

        #region ExitButton

        private class ExitButton : ReactiveComponent {
            public Action? OnClick;

            protected override GameObject Construct() {
                var color = UIStyle.GlowingButtonColorSet.Color;
                var activeColor = UIStyle.GlowingButtonColorSet.GetColor(GraphicState.Active);

                var progress = RememberAnimated(0f, 10.fact);

                return new Background {
                        Sprite = BundleLoader.ClosedDoorIcon,
                        Material = BundleLoader.UIAdditiveGlowMaterial,
                        PreserveAspect = true,
                        sColor = progress.Map(x => Color.Lerp(color, Color.clear, x)),
                        Children = {
                            new Image {
                                Sprite = BundleLoader.OpenedDoorIcon,
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                PreserveAspect = true,
                                RaycastTarget = false,
                                sColor = progress.Map(x => Color.Lerp(Color.clear, activeColor, x))
                            }.WithRectExpand()
                        }
                    }
                    .WithPointerEvents(
                        onEnter: _ => progress.TargetValue = 1f,
                        onLeave: _ => progress.TargetValue = 0f,
                        onDown: _ => OnClick?.Invoke()
                    )
                    .WithHoverScale(1f, 1.2f)
                    .Use();
            }
        }

        #endregion

        #region SettingsPanel

        public interface ISettingsPanel {
            void Present();
            void Dismiss();
        }

        #endregion

        #region Callbacks

        private void HandlePlayButtonClicked() {
            if (!_pauseController!.IsPaused) {
                _pauseController.Pause();
            } else {
                _pauseController.Resume();
            }
        }

        private void HandleSettingsButtonClicked() {
            var presented = !_settingsButtonState.IsActive;
            _settingsButtonState.IsActive = presented;
            if (presented) {
                _settingsPanel!.Present();
            } else {
                _settingsPanel!.Dismiss();
            }
        }

        private void HandleExitButtonClicked() {
            _finishController?.Exit();
        }

        private void HandlePauseStateChanged(bool paused) {
            RefreshPlayButton(paused);
        }

        #endregion
    }
}
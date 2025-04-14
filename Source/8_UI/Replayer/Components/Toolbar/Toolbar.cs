using System;
using UnityEngine;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using Image = Reactive.BeatSaber.Components.Image;
using ImageButton = Reactive.BeatSaber.Components.ImageButton;
using ImageLayout = Reactive.BeatSaber.Components.ImageLayout;
using Label = Reactive.BeatSaber.Components.Label;

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
        private ImageButton _playButton = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new ExitButton {
                            OnClick = HandleExitButtonClicked
                        }
                        .AsFlexItem(grow: 1f)
                        .InBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                        .AsFlexGroup(padding: new() { top = 1.5f, bottom = 1.5f, right = 1f })
                        .AsFlexItem(basis: 10f),
                    //toolbar
                    new ImageLayout {
                        Children = {
                            //play button
                            new ImageButton {
                                OnClick = HandlePlayButtonClicked,
                                Colors = UIStyle.ButtonColorSet,
                                Image = {
                                    PreserveAspect = true,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial
                                }
                            }.Bind(ref _playButton).AsFlexItem(
                                basis: 5f,
                                margin: new() { left = 1f, right = 1f }
                            ),
                            //timeline
                            new Timeline().Bind(ref _timeline).AsFlexItem(
                                grow: 1f,
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
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.SettingsIcon,
                                    Material = BundleLoader.Materials.uiAdditiveGlowMaterial,
                                    PreserveAspect = true
                                },
                                Colors = UIStyle.ButtonColorSet,
                                OnStateChanged = HandleSettingsButtonStateChanged,
                                Latching = true
                            }.AsFlexItem(
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
                    ).AsFlexItem(grow: 1f)
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
            _playButton.Image.Sprite = paused ? playSprite : pauseSprite;
        }

        #endregion

        #region ExitButton

        private class ExitButton : ReactiveComponent {
            public Action? OnClick;

            protected override GameObject Construct() {
                static Image CreateImage(Sprite sprite) {
                    return new Image {
                        Sprite = sprite,
                        Material = BundleLoader.UIAdditiveGlowMaterial,
                        PreserveAspect = true
                    }.WithRectExpand();
                }

                var color = UIStyle.ButtonColorSet.Color;
                var activeColor = UIStyle.ButtonColorSet.ActiveColor;
                var progress = RememberAnimated(0f, 10.fact());

                return new Clickable {
                    OnClick = () => OnClick?.Invoke(),
                    Children = {
                        CreateImage(BundleLoader.ClosedDoorIcon)
                            .WithEffect(progress, (x, y) => x.Color = Color.Lerp(color, Color.clear, y)),

                        CreateImage(BundleLoader.OpenedDoorIcon)
                            .WithEffect(progress, (x, y) => x.Color = Color.Lerp(Color.clear, activeColor, y))
                    }
                }.WithListener(
                    x => x.IsHovered,
                    x => progress.Value = x ? 1f : 0f
                ).WithScaleAnimation(1f, 1.2f).Use();
            }
        }

        #endregion

        #region SettingsPanel

        public interface ISettingsPanel {
            void Present();
            void Dismiss();
        }

        private bool _settingsPanelPresented;

        private void RefreshSettingsPanel(bool state) {
            if (state == _settingsPanelPresented) return;
            if (state) {
                _settingsPanel!.Present();
            } else {
                _settingsPanel!.Dismiss();
            }
            _settingsPanelPresented = state;
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

        private void HandleSettingsButtonStateChanged(bool state) {
            RefreshSettingsPanel(state);
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
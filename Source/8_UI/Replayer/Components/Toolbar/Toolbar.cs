using UnityEngine;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;

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
            ISettingsPanel settingsPanel
        ) {
            if (_pauseController != null) {
                _pauseController.PauseStateChangedEvent -= HandlePauseStateChanged;
            }
            _pauseController = pauseController;
            _finishController = finishController;
            _beatmapTimeController = timeController;
            _settingsPanel = settingsPanel;

            _pauseController.PauseStateChangedEvent += HandlePauseStateChanged;
            _timeline.Setup(playersManager, pauseController, timeController);
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
            return new Dummy {
                Children = {
                    new ExitButton()
                        .WithClickListener(HandleExitButtonClicked)
                        .AsFlexItem(grow: 1f)
                        .InBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                        .AsFlexGroup(padding: new() { top = 1.5f, bottom = 1.5f, right = 1f })
                        .AsFlexItem(basis: 10f),
                    //toolbar
                    new Image {
                        Children = {
                            //play button
                            new ImageButton {
                                Image = {
                                    PreserveAspect = true
                                }
                            }.WithClickListener(HandlePlayButtonClicked).Bind(ref _playButton).AsFlexItem(
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
                                alignSelf: Align.Center
                            ),
                            //settings button
                            new ImageButton {
                                Image = {
                                    Sprite = BundleLoader.SettingsIcon,
                                    PreserveAspect = true
                                },
                                Sticky = true
                            }.WithStateListener(HandleSettingsButtonStateChanged).AsFlexItem(
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

        private class ExitButton : ColoredButton {
            private Image _image1 = null!;
            private Image _image2 = null!;

            protected override void ApplyColor(Color color) {
                _image1.Color = Color.Lerp(Colors!.Color, Color.clear, AnimationProgress);
                _image2.Color = color.ColorWithAlpha(AnimationProgress);
            }

            protected override GameObject Construct() {
                static Image CreateImage(Sprite sprite) {
                    return new Image {
                        Sprite = sprite,
                        Material = BundleLoader.UIAdditiveGlowMaterial,
                        PreserveAspect = true
                    }.WithRectExpand();
                }

                CreateImage(BundleLoader.ClosedDoorIcon).Bind(ref _image1);
                CreateImage(BundleLoader.OpenedDoorIcon).Bind(ref _image2);
                var parent = base.Construct();
                _image1.Use(parent);
                _image2.Use(parent);
                return parent;
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
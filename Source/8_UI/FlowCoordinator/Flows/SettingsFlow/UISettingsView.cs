using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class UISettingsView : ReactiveComponent {
        #region Setup

        private BeatLeaderHubMenuButtonsTheme? _menuButtonsTheme;

        public void Setup(BeatLeaderHubMenuButtonsTheme menuButtonsTheme) {
            _menuButtonsTheme = menuButtonsTheme;
            _replayManagerButtonColorPicker.Color = menuButtonsTheme.ReplayManagerButtonColors.HoveredColor;
            _battleRoyaleButtonColorPicker.Color = menuButtonsTheme.BattleRoyaleButtonColors.HoveredColor;
            _settingsButtonColorPicker.Color = menuButtonsTheme.SettingsButtonColors.HoveredColor;
        }

        #endregion

        #region Construct

        private ColorPicker _replayManagerButtonColorPicker = null!;
        private ColorPicker _battleRoyaleButtonColorPicker = null!;
        private ColorPicker _settingsButtonColorPicker = null!;

        protected override GameObject Construct() {
            static ReactiveComponentBase CreateContainer(string name, params ILayoutItem[] children) {
                return new Image {
                    Children = {
                        new Label {
                            Text = name
                        }.AsFlexItem(size: new() { y = "auto" })
                    }
                }.AsBlurBackground(pixelsPerUnit: 10f).With(
                    x => x.Children.AddRange(children)
                ).AsFlexGroup(
                    direction: FlexDirection.Column,
                    gap: 1f,
                    padding: new() { left = 2f, bottom = 2f, right = 2f, top = 1f }
                ).AsFlexItem();
            }

            return new Dummy {
                Children = {
                    CreateContainer(
                        "Hub Button Colors",
                        //
                        new ColorPicker()
                            .WithListener(
                                x => x.Color,
                                HandleReplayManagerButtonColorChanged
                            )
                            .Bind(ref _replayManagerButtonColorPicker)
                            .InNamedRail("Replay Manager"),
                        //
                        new ColorPicker()
                            .WithListener(
                                x => x.Color,
                                HandleBattleRoyaleButtonColorChanged
                            )
                            .Bind(ref _battleRoyaleButtonColorPicker)
                            .InNamedRail("Battle Royale"),
                        //
                        new ColorPicker()
                            .WithListener(
                                x => x.Color,
                                HandleSettingsButtonColorChanged
                            )
                            .Bind(ref _settingsButtonColorPicker)
                            .InNamedRail("Settings")
                    ).AsFlexItem(size: new() { x = 60f })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.FlexStart,
                alignItems: Align.Center,
                gap: 1f
            ).Use();
        }

        #endregion

        #region Callbacks

        private void HandleReplayManagerButtonColorChanged(Color color) {
            if (_menuButtonsTheme == null) return;
            _menuButtonsTheme.ReplayManagerButtonColors.HoveredColor = color;
        }

        private void HandleBattleRoyaleButtonColorChanged(Color color) {
            if (_menuButtonsTheme == null) return;
            _menuButtonsTheme.BattleRoyaleButtonColors.HoveredColor = color;
        }

        private void HandleSettingsButtonColorChanged(Color color) {
            if (_menuButtonsTheme == null) return;
            _menuButtonsTheme.SettingsButtonColors.HoveredColor = color;
        }

        #endregion
    }
}
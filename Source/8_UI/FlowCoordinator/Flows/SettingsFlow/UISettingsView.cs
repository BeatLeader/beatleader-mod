using System;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class UISettingsView : ReactiveComponent {
        #region Setup

        private BeatLeaderHubMenuButtonsTheme? _menuButtonsTheme;
        private ReplayManagerSearchTheme? _replayManagerTheme;

        public void Setup(BeatLeaderHubTheme theme) {
            _menuButtonsTheme = theme.MenuButtonsTheme;
            _replayManagerButtonColorPicker.Color = _menuButtonsTheme.ReplayManagerButtonColors.HoveredColor;
            _battleRoyaleButtonColorPicker.Color = _menuButtonsTheme.BattleRoyaleButtonColors.HoveredColor;
            _settingsButtonColorPicker.Color = _menuButtonsTheme.SettingsButtonColors.HoveredColor;
            //
            _replayManagerTheme = theme.ReplayManagerSearchTheme;
            _replayManagerSearchColorPicker.Color = _replayManagerTheme.SearchHighlightColor;
            var bold = _replayManagerTheme.SearchHighlightStyle.HasFlag(FontStyle.Bold);
            _replayManagerSearchBoldToggle.SetActive(bold, false, true);
        }

        #endregion

        #region Construct

        private ColorPicker _replayManagerButtonColorPicker = null!;
        private ColorPicker _battleRoyaleButtonColorPicker = null!;
        private ColorPicker _settingsButtonColorPicker = null!;

        private ColorPicker _replayManagerSearchColorPicker = null!;
        private Toggle _replayManagerSearchBoldToggle = null!;

        protected override GameObject Construct() {
            static ReactiveComponent CreateContainer(string name, params ILayoutItem[] children) {
                return new Background {
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

            static ReactiveComponent CreateColorPicker(string name, Action<Color> callback, ref ColorPicker picker) {
                return new ColorPicker()
                    .WithListener(x => x.Color, callback)
                    .Bind(ref picker)
                    .InNamedRail(name);
            }

            return new Layout {
                Children = {
                    CreateContainer(
                        "Hub Button Colors",
                        //
                        CreateColorPicker(
                            "Replay Manager",
                            HandleReplayManagerButtonColorChanged,
                            ref _replayManagerButtonColorPicker
                        ),
                        //
                        CreateColorPicker(
                            "Battle Royale",
                            HandleBattleRoyaleButtonColorChanged,
                            ref _battleRoyaleButtonColorPicker
                        ),
                        //
                        CreateColorPicker(
                            "Settings",
                            HandleSettingsButtonColorChanged,
                            ref _settingsButtonColorPicker
                        )
                    ).AsFlexItem(size: new() { x = 60f }),
                    //
                    CreateContainer(
                        "Replay Manager Search Highlight",
                        //
                        CreateColorPicker(
                            "Color",
                            HandleReplayManagerSearchColorChanged,
                            ref _replayManagerSearchColorPicker
                        ),
                        //
                        new Toggle()
                            .WithListener(
                                x => x.Active,
                                HandleReplayManagerSearchBoldChanged
                            )
                            .Bind(ref _replayManagerSearchBoldToggle)
                            .InNamedRail("Bold")
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

        private void HandleReplayManagerSearchColorChanged(Color color) {
            if (_replayManagerTheme == null) return;
            _replayManagerTheme.SearchHighlightColor = color;
        }

        private void HandleReplayManagerSearchBoldChanged(bool bold) {
            if (_replayManagerTheme == null) return;
            if (bold) {
                _replayManagerTheme.SearchHighlightStyle |= FontStyle.Bold;
            } else {
                _replayManagerTheme.SearchHighlightStyle &= ~FontStyle.Bold;
            }
        }

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
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

        private Color _initialReplayManagerButtonColor;
        private Color _initialBattleRoyaleButtonColor;
        private Color _initialSettingsButtonColor;
        private Color _initialAvatarButtonColor;
        private Color _initialSearchHighlightColor;
        private FontStyle _initialSearchHighlightStyle;

        public void Setup(BeatLeaderHubTheme theme) {
            _menuButtonsTheme = theme.MenuButtonsTheme;
            _replayManagerButtonColorPicker.Color = _menuButtonsTheme.ReplayManagerButtonColors.HoveredColor;
            _battleRoyaleButtonColorPicker.Color = _menuButtonsTheme.BattleRoyaleButtonColors.HoveredColor;
            _settingsButtonColorPicker.Color = _menuButtonsTheme.SettingsButtonColors.HoveredColor;
            _avatarButtonColorPicker.Color = _menuButtonsTheme.EditAvatarButtonColors.HoveredColor;
            //
            _replayManagerTheme = theme.ReplayManagerSearchTheme;
            _replayManagerSearchColorPicker.Color = _replayManagerTheme.SearchHighlightColor;
            var bold = _replayManagerTheme.SearchHighlightStyle.HasFlag(FontStyle.Bold);
            _replayManagerSearchBoldToggle.SetActive(bold, false, true);
            //
            SaveInitialValues();
        }

        public void CancelSelection() {
            if (_menuButtonsTheme != null) {
                _menuButtonsTheme.ReplayManagerButtonColors.HoveredColor = _initialReplayManagerButtonColor;
                _menuButtonsTheme.BattleRoyaleButtonColors.HoveredColor = _initialBattleRoyaleButtonColor;
                _menuButtonsTheme.SettingsButtonColors.HoveredColor = _initialSettingsButtonColor;
                _menuButtonsTheme.EditAvatarButtonColors.HoveredColor = _initialAvatarButtonColor;
            }
            if (_replayManagerTheme != null) {
                _replayManagerTheme.SearchHighlightColor = _initialSearchHighlightColor;
                _replayManagerTheme.SearchHighlightStyle = _initialSearchHighlightStyle;
            }

            _replayManagerButtonColorPicker.Color = _initialReplayManagerButtonColor;
            _battleRoyaleButtonColorPicker.Color = _initialBattleRoyaleButtonColor;
            _settingsButtonColorPicker.Color = _initialSettingsButtonColor;
            _avatarButtonColorPicker.Color = _initialAvatarButtonColor;
            _replayManagerSearchColorPicker.Color = _initialSearchHighlightColor;
            _replayManagerSearchBoldToggle.SetActive(_initialSearchHighlightStyle.HasFlag(FontStyle.Bold), false, true);
        }

        private void SaveInitialValues() {
            if (_menuButtonsTheme != null) {
                _initialReplayManagerButtonColor = _menuButtonsTheme.ReplayManagerButtonColors.HoveredColor;
                _initialBattleRoyaleButtonColor = _menuButtonsTheme.BattleRoyaleButtonColors.HoveredColor;
                _initialSettingsButtonColor = _menuButtonsTheme.SettingsButtonColors.HoveredColor;
                _initialAvatarButtonColor = _menuButtonsTheme.EditAvatarButtonColors.HoveredColor;
            }
            if (_replayManagerTheme != null) {
                _initialSearchHighlightColor = _replayManagerTheme.SearchHighlightColor;
                _initialSearchHighlightStyle = _replayManagerTheme.SearchHighlightStyle;
            }
        }

        #endregion

        #region Construct

        private ColorPicker _replayManagerButtonColorPicker = null!;
        private ColorPicker _battleRoyaleButtonColorPicker = null!;
        private ColorPicker _settingsButtonColorPicker = null!;
        private ColorPicker _avatarButtonColorPicker = null!;

        private ColorPicker _replayManagerSearchColorPicker = null!;
        private Toggle _replayManagerSearchBoldToggle = null!;

        protected override GameObject Construct() {
            static ReactiveComponent CreateContainer(string name, params ILayoutItem[] children) {
                return new Layout {
                        Children = {
                            new Label {
                                    Text = name,
                                    FontSize = 5f
                                }
                                .AsFlexItem(size: new() { y = "auto" })
                        }
                    }
                    .With(x => x.Children.AddRange(children))
                    .AsFlexGroup(
                        direction: FlexDirection.Column,
                        gap: 1f,
                        padding: new() { left = 2f, bottom = 2f, right = 2f, top = 1f }
                    )
                    .AsFlexItem();
            }

            static ReactiveComponent CreateColorPicker(string name, Action<Color> callback, ref ColorPicker picker) {
                return new ColorPicker()
                    .AsFlexItem(size: new() { y = 6f })
                    .WithListener(x => x.Color, callback)
                    .Bind(ref picker)
                    .InNamedRail(name);
            }

            return 
                new Layout {
                    Children = {
                        new ScrollArea {
                            ScrollContent = new Layout {
                                Children = {
                                    CreateContainer(
                                        "Hub Buttons",
                        
                                        // Replay Manager button
                                        CreateColorPicker(
                                            "Replay Manager",
                                            HandleReplayManagerButtonColorChanged,
                                            ref _replayManagerButtonColorPicker
                                        ),
                        
                                        // Battle Royale button
                                        CreateColorPicker(
                                            "Battle Royale",
                                            HandleBattleRoyaleButtonColorChanged,
                                            ref _battleRoyaleButtonColorPicker
                                        ),

                                        // Settings button
                                        CreateColorPicker(
                                            "Settings",
                                            HandleSettingsButtonColorChanged,
                                            ref _settingsButtonColorPicker
                                        ),

                                        // Edit avatar button
                                        CreateColorPicker(
                                            "Edit Avatar",
                                            HandleAvatarButtonColorChanged,
                                            ref _avatarButtonColorPicker
                                        )
                                    ),
                                    //
                                    CreateContainer(
                                        "Replay Search",
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
                                    )
                                }
                            }.AsFlexGroup(
                                direction: FlexDirection.Column,
                                justifyContent: Justify.FlexStart,
                                alignItems: Align.Stretch,
                                gap: 1f,
                                constrainVertical: false
                            ),
                        }.AsFlexItem(flexGrow: 1f).Export(out var scrollArea),
                    
                        new Scrollbar()
                            .AsFlexItem()
                            .With(x => scrollArea.Scrollbar = x)
                    }
                }
                .AsFlexGroup(gap: 1f)
                .AsFlexItem()
                .Use();
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

        private void HandleAvatarButtonColorChanged(Color color) {
            if (_menuButtonsTheme == null) return;
            _menuButtonsTheme.EditAvatarButtonColors.HoveredColor = color;
        }

        #endregion
    }
}
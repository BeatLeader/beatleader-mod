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
            _replayManagerSearchBoldToggle.Toggled = _replayManagerTheme.SearchHighlightStyle.HasFlag(FontStyle.Bold);
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
            _replayManagerSearchBoldToggle.Toggled = _initialSearchHighlightStyle.HasFlag(FontStyle.Bold);
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

        #region ColorPicker

        private class ColorPickerControl : ReactiveComponent {
            public Color Color {
                get => _savedColor;
                set {
                    _savedColor = value;
                    _context.SelectedColor = value;
                }
            }

            public Action<Color>? OnColorChanged { get; set; }

            private readonly BsColorPickerContext _context = new();
            private Color _savedColor;

            protected override GameObject Construct() {
                _context.AddCallback(this, static (x, y) => x.HandleContextUpdated(y), lazy: true);

                return new Layout {
                    FlexController = {
                        AlignItems = Align.Stretch
                    },

                    Children = {
                        new BsColorPicker {
                            FlexItem = {
                                Size = new() { y = YogaValue.Auto }
                            },

                            Context = _context
                        }.Export(out var picker),

                        new BsColorCircleModal {
                            Context = _context,
                            Title = "Select Color",
                            CircleSize = 54.pt,
                            PlacementAnchor = picker.ContentTransform,
                            PlacementData = new() { Placement = RelativePlacement.Center }
                        }
                    }
                }.Use();
            }

            private void HandleContextUpdated(BsColorPickerContext context) {
                if (context.SelectedColor == _savedColor) {
                    return;
                }

                _savedColor = context.SelectedColor;
                OnColorChanged?.Invoke(_savedColor);
            }
        }

        #endregion

        #region Construct

        private ColorPickerControl _replayManagerButtonColorPicker = null!;
        private ColorPickerControl _battleRoyaleButtonColorPicker = null!;
        private ColorPickerControl _settingsButtonColorPicker = null!;
        private ColorPickerControl _avatarButtonColorPicker = null!;

        private ColorPickerControl _replayManagerSearchColorPicker = null!;
        private BsToggle _replayManagerSearchBoldToggle = null!;

        protected override GameObject Construct() {
            static ReactiveComponent CreateContainer(string name, params ILayoutItem[] children) {
                return new Layout {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        Gap = 1f,
                        Padding = new() { left = 2f, bottom = 2f, right = 2f, top = 1f }
                    },

                    Children = {
                        new Label {
                            FlexItem = {
                                Size = new() { y = "auto" }
                            },

                            Text = name,
                            FontSize = 5f
                        }
                    }
                }.With(x => x.Children.AddRange(children));
            }

            static ReactiveComponent CreateColorPicker(string name, Action<Color> callback, ref ColorPickerControl picker) {
                return new ColorPickerControl {
                        FlexItem = {
                            Size = new() { y = 6f }
                        },

                        OnColorChanged = callback
                    }
                    .Bind(ref picker)
                    .InNamedRail(name);
            }

            var scrollContext = new ScrollContext();

            return
                new Layout {
                    FlexController = {
                        Gap = 1f
                    },

                    Children = {
                        new ScrollArea {
                            FlexItem = {
                                FlexGrow = 1f
                            },

                            ScrollContext = scrollContext,
                            LineSize = 1f,
                            ScrollContent = new Layout {
                                FlexController = {
                                    FlexDirection = FlexDirection.Column,
                                    JustifyContent = Justify.FlexStart,
                                    AlignItems = Align.Stretch,
                                    Gap = 1f,
                                    ConstrainVertical = false
                                },

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
                                        new BsToggle {
                                                OnToggleStateChanged = HandleReplayManagerSearchBoldChanged
                                            }
                                            .Bind(ref _replayManagerSearchBoldToggle)
                                            .InNamedRail("Bold")
                                    )
                                }
                            },
                        },

                        new Scrollbar {
                            ScrollContext = scrollContext,
                            HideIfNothingToScroll = true
                        }
                    }
                }.Use();
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
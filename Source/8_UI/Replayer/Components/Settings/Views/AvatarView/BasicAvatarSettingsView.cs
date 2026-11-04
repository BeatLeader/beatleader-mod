using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class BasicAvatarSettingsView : ReactiveComponent {
        #region Setup

        private BodySettings? _bodySettings;

        public void Setup(BodySettings bodySettings, bool battleRoyaleEnabled) {
            if (_bodySettings != null) {
                return;
            }

            _bodySettings = bodySettings;

            var views = new Dictionary<int, IReactiveComponent> {
                [0] = CreateBasicView(bodySettings.RequireConfig<BasicBodySettings>(), null)
            };

            if (battleRoyaleEnabled) {
                views[1] = CreateRoyaleView(bodySettings.RequireConfig<BattleRoyaleBodySettings>());
                _segmentedControl.Enabled = true;
            }

            _keyedContainer.Views = views;
        }

        #endregion

        #region Construct

        private static readonly BsSegmentedControlColors segmentedControlColors = BeatSaberStyle.BsSegmentedControlColors with {
            CellBackgroundColors = new ColorSet {
                Color = new(0.1f, 0.1f, 0.1f, 1f),
                HoveredColor = new(0.28f, 0.28f, 0.28f, 1f),
                ActiveColor = new(0.19f, 0.19f, 0.19f, 1f)
            }
        };

        private ScrollContext _scrollContext = null!;
        private BsSegmentedControl<int> _segmentedControl = null!;
        private KeyedContainer<int> _keyedContainer = null!;

        protected override GameObject Construct() {
            _scrollContext = new ScrollContext();

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Gap = 1f
                },

                Children = {
                    new BsSegmentedControl<int> {
                            FlexItem = {
                                FlexBasis = 6f
                            },

                            Items = new Dictionary<int, BsSegmentedControlItem> {
                                [0] = new("Main Player", null),
                                [1] = new("Other Players", null)
                            },
                            Key = 0,
                            Skew = 0f,
                            Colors = segmentedControlColors,
                            OnKeyChanged = HandleSegmentedControlKeyChanged
                        }
                        .With(x => x.Enabled = false)
                        .Bind(ref _segmentedControl),

                    new Layout {
                        FlexController = {
                            Gap = 1f
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        Children = {
                            new ScrollArea {
                                FlexItem = {
                                    FlexGrow = 1f
                                },

                                ScrollContext = _scrollContext,
                                ScrollContent = new KeyedContainer<int> {
                                    FlexController = {
                                        JustifyContent = Justify.FlexStart,
                                        ConstrainVertical = false
                                    },

                                    Key = 0,
                                    Views = new Dictionary<int, IReactiveComponent>()
                                }.Bind(ref _keyedContainer),
                                LineSize = 1f
                            },

                            new Scrollbar {
                                ScrollContext = _scrollContext,
                                HideIfNothingToScroll = true,
                                WithinLayoutIfDisabled = false
                            }
                        }
                    }
                }
            }.Use();
        }

        private void HandleSegmentedControlKeyChanged(int key) {
            _keyedContainer.Key = key;
            _scrollContext.ScrollTo(0f, immediately: true);
        }

        #endregion

        #region Views

        private Layout CreateRoyaleView(BattleRoyaleBodySettings settings) {
            return CreateBasicView(
                settings,
                CreateContainer(
                    CreateToggle(
                        settings,
                        "Enable Trails",
                        settings.TrailEnabled,
                        x => settings.TrailEnabled = x
                    ),
                    CreateSlider(
                        settings,
                        from: 0.1f,
                        to: 2,
                        step: 0.1f,
                        "Trail Length",
                        settings.TrailLength,
                        x => settings.TrailLength = x
                    ),
                    CreateSlider(
                        settings,
                        from: 0.05f,
                        to: 1,
                        step: 0.05f,
                        "Trail Opacity",
                        settings.TrailOpacity,
                        x => settings.TrailOpacity = x
                    )
                )
            );
        }

        private Layout CreateBasicView(BasicBodySettings settings, ILayoutItem? additionalComponent) {
            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    AlignItems = Align.Stretch,
                    Gap = 2f
                },

                FlexItem = {
                    Size = new() { x = 100.pct }
                },

                Children = {
                    CreateContainer(
                        CreateToggle(
                            settings,
                            "Head",
                            settings.HeadEnabled,
                            x => settings.HeadEnabled = x
                        ),
                        CreateToggle(
                            settings,
                            "Torso",
                            settings.TorsoEnabled,
                            x => settings.TorsoEnabled = x
                        ),
                        CreateToggle(
                            settings,
                            "Left Hand",
                            settings.LeftHandEnabled,
                            x => settings.LeftHandEnabled = x
                        ),
                        CreateToggle(
                            settings,
                            "Right Hand",
                            settings.RightHandEnabled,
                            x => settings.RightHandEnabled = x
                        )
                    ),

                    CreateContainer(
                        CreateToggle(
                            settings,
                            "Left Saber",
                            settings.LeftSaberEnabled,
                            x => settings.LeftSaberEnabled = x
                        ),
                        CreateToggle(
                            settings,
                            "Right Saber",
                            settings.RightSaberEnabled,
                            x => settings.RightSaberEnabled = x
                        )
                    )
                }
            }.With(
                x => {
                    if (additionalComponent != null) {
                        x.Children.Add(additionalComponent);
                    }
                }
            );
        }

        private static Background CreateContainer(params ILayoutItem[] components) {
            var image = new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    AlignItems = Align.Stretch,
                    Padding = 2f,
                    Gap = 1f
                }
            }.AsBackground(
                    sprite: BundleLoader.Sprites.background,
                    color: new(0.1f, 0.1f, 0.1f, 1f),
                    pixelsPerUnit: 7f
                );

            image.Children.AddRange(components);

            return image;
        }

        private NamedRail CreateToggle(BasicBodySettings settings, string name, bool initial, Action<bool> callback) {
            return new BsToggle {
                Toggled = initial,
                OnToggleStateChanged = x => {
                    callback(x);
                    _bodySettings!.NotifyConfigUpdated(settings);
                }
            }.InNamedRail(name);
        }

        private NamedRail CreateSlider(
            BasicBodySettings settings,
            float from,
            float to,
            float step,
            string name,
            float initial,
            Action<float> callback
        ) {
            return new BsSlider {
                MinValue = from,
                MaxValue = to,
                ValueStep = step,
                Value = initial,
                OnValueChanged = x => {
                    callback(x);
                    _bodySettings!.NotifyConfigUpdated(settings);
                }
            }.InNamedRail(name);
        }

        #endregion
    }
}
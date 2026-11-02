using System;
using System.Collections.Generic;
using BeatLeader.Models;
using Reactive;
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
                _segmentedControlContainer.Enabled = true;
            }

            _keyedContainer.Views = views;
        }

        #endregion

        #region Construct

        private ScrollContext _scrollContext = null!;
        private Image _segmentedControlContainer = null!;
        private KeyedContainer<int> _keyedContainer = null!;

        protected override GameObject Construct() {
            _scrollContext = new ScrollContext();

            return new Layout {
                Children = {
                    new BsSegmentedControl<int> {
                            Items = new Dictionary<int, BsSegmentedControlItem> {
                                [0] = new("Main Player", null),
                                [1] = new("Other Players", null)
                            },
                            Key = 0,
                            OnKeyChanged = HandleSegmentedControlKeyChanged
                        }
                        .AsFlexItem(size: 100.pct)
                        .WithRectExpand()
                        .InBackground(
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 10f
                        )
                        .AsFlexItem(basis: 6f)
                        .With(x => x.Enabled = false)
                        .Bind(ref _segmentedControlContainer),

                    new Layout {
                        Children = {
                            new ScrollArea {
                                    ScrollContext = _scrollContext,
                                    ScrollContent = new KeyedContainer<int> {
                                            Key = 0,
                                            Views = new Dictionary<int, IReactiveComponent>()
                                        }
                                        .AsFlexGroup(justifyContent: Justify.FlexStart, constrainVertical: false)
                                        .AsFlexItem()
                                        .Bind(ref _keyedContainer),
                                    LineSize = 1f
                                }
                                .AsFlexItem(flexGrow: 1f),

                            new Scrollbar {
                                ScrollContext = _scrollContext,
                                HideIfNothingToScroll = true,
                                WithinLayoutIfDisabled = false
                            }
                        }
                    }.AsFlexGroup(gap: 1f).AsFlexItem(flexGrow: 1f)
                }
            }.AsFlexGroup(direction: FlexDirection.Column, gap: 1f).Use();
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
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                alignItems: Align.Stretch,
                gap: 2f
            ).AsFlexItem(
                size: new() { x = "100%" }
            ).With(
                x => {
                    if (additionalComponent != null) {
                        x.Children.Add(additionalComponent);
                    }
                }
            );
        }

        private static Background CreateContainer(params ILayoutItem[] components) {
            var image = new Background()
                .AsBackground(
                    sprite: BundleLoader.Sprites.background,
                    color: new(0.1f, 0.1f, 0.1f, 1f),
                    pixelsPerUnit: 7f
                ).AsFlexGroup(
                    direction: FlexDirection.Column,
                    alignItems: Align.Stretch,
                    padding: 2f,
                    gap: 1f
                ).AsFlexItem();

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
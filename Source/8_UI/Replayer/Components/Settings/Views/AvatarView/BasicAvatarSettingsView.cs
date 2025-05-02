using System;
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

            var basicSettings = bodySettings.RequireConfig<BasicBodySettings>();
            _basicView = CreateBasicView(basicSettings, null);

            _keyedContainer.Items[0] = _basicView;

            if (battleRoyaleEnabled) {
                var royaleSettings = bodySettings.RequireConfig<BattleRoyaleBodySettings>();
                _royaleView = CreateRoyaleView(royaleSettings);

                _keyedContainer.Items[1] = _royaleView;
                _segmentedControlContainer.Enabled = true;
            }
        }

        #endregion

        #region Construct

        private ScrollArea _scrollArea = null!;
        private Image _segmentedControlContainer = null!;
        private TextSegmentedControl<int> _segmentedControl = null!;
        private KeyedContainer<int> _keyedContainer = null!;

        private Layout? _basicView;
        private Layout? _royaleView;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new TextSegmentedControl<int> {
                            Items = {
                                [0] = "Standard",
                                [1] = "Battle Royale"
                            }
                        }
                        .WithRectExpand()
                        .Bind(ref _segmentedControl)
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
                                    ScrollContent = new KeyedContainer<int> {
                                            Control = _segmentedControl
                                        }
                                        .AsFlexGroup(justifyContent: Justify.FlexStart, constrainVertical: false)
                                        .AsFlexItem()
                                        .Bind(ref _keyedContainer)
                                }
                                .AsFlexItem(flexGrow: 1f)
                                .Export(out var area)
                                .Bind(ref _scrollArea),

                            new Scrollbar {
                                WithinLayoutIfDisabled = false
                            }.With(x => area.Scrollbar = x)
                        }
                    }.AsFlexGroup(gap: 1f).AsFlexItem(flexGrow: 1f)
                }
            }.AsFlexGroup(direction: FlexDirection.Column, gap: 1f).Use();
        }

        protected override void OnInitialize() {
            _segmentedControl.SelectedKeyChangedEvent += RefreshScrollArea;
        }

        protected override void OnDestroy() {
            _segmentedControl.SelectedKeyChangedEvent -= RefreshScrollArea;
        }

        private void RefreshScrollArea(int _) {
            _scrollArea.ScrollToStart(true);
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
            return new Toggle()
                .With(x => x.SetActive(initial, false))
                .WithListener(
                    x => x.Active,
                    x => {
                        callback(x);
                        _bodySettings!.NotifyConfigUpdated(settings);
                    }
                )
                .InNamedRail(name);
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
            return new Slider {
                    ValueRange = new(from, to),
                    ValueStep = step,
                    Value = initial,
                }
                .WithListener(
                    x => x.Value,
                    x => {
                        callback(x);
                        _bodySettings!.NotifyConfigUpdated(settings);
                    }
                )
                .InNamedRail(name);
        }

        #endregion
    }
}
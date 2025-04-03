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
                _segmentedControl.Enabled = true;
            }
        }

        #endregion

        #region Construct

        private ScrollArea _scrollArea = null!;
        private TextSegmentedControl<int> _segmentedControl = null!;
        private KeyedContainer<int> _keyedContainer = null!;

        private Dummy? _basicView;
        private Dummy? _royaleView;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new TextSegmentedControl<int> {
                        Enabled = false,
                        Items = {
                            [0] = "Standard",
                            [1] = "Battle Royale"
                        }
                    }.AsFlexItem(basis: 6f).Bind(ref _segmentedControl),

                    new ScrollArea {
                            ScrollContent = new KeyedContainer<int> {
                                    Control = _segmentedControl,
                                }
                                .AsRootFlexGroup()
                                .AsFlexItem(size: new() { y = "auto" })
                                .Bind(ref _keyedContainer)
                        }
                        .AsFlexItem(grow: 1f)
                        .Export(out var area)
                        .Bind(ref _scrollArea),

                    new Scrollbar {
                        WithinLayoutIfDisabled = false
                    }.With(x => area.Scrollbar = x)
                }
            }.AsFlexGroup(alignItems: Align.Stretch, gap: 1f).Use();
        }

        #endregion

        #region Views

        private Dummy CreateRoyaleView(BattleRoyaleBodySettings settings) {
            return CreateBasicView(settings, null);
        }

        private Dummy CreateBasicView(BasicBodySettings settings, ILayoutItem? additionalComponent) {
            return new Dummy {
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

        private static Image CreateContainer(params ILayoutItem[] components) {
            var image = new Image().AsBackground(
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
            return new Toggle {
                    Active = initial
                }
                .WithListener(
                    x => x.Active,
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
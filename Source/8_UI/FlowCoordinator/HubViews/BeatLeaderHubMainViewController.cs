using System;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderHubMainViewController : ViewController {
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;
        [Inject] private readonly ReplayManagerFlowCoordinator _replayManagerFlowCoordinator = null!;
        [Inject] private readonly BattleRoyaleFlowCoordinator _battleRoyaleFlowCoordinator = null!;
        [Inject] private readonly BeatLeaderSettingsFlowCoordinator _settingsFlowCoordinator = null!;
        [Inject] private readonly BeatLeaderHubTheme _beatLeaderHubTheme = null!;

        private QuickMiniProfile _quickMiniProfile = null!;

        [StateGen]
        private BsAeroButton CreateActionButton(
            string text,
            Sprite icon,
            IState<SerializableColorSet> colorSet,
            YogaFrame position,
            float iconMargin,
            float gap,
            FlowCoordinator flowCoordinator,
            Action? beforePresent = null
        ) {
            return new BsAeroButton {
                FlexController = {
                    Padding = new() { left = 2f, right = 2f, top = iconMargin }
                },

                FlexItem = {
                    Size = 30f,
                    Position = position,
                    PositionType = PositionType.Absolute
                },

                Skew = 0f,
                sColors = colorSet.Map(static x => UIStyle.AeroButtonColors(x.ToColorSet())),
                OnClick = () => {
                    beforePresent?.Invoke();

                    _beatLeaderHubFlowCoordinator.PresentFlowCoordinator(
                        flowCoordinator,
                        animationDirection: AnimationDirection.Vertical
                    );
                },
                ConstructContent = (color, _) => new Layout {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        Gap = gap
                    },

                    FlexItem = {
                        FlexGrow = 1f
                    },

                    Children = {
                        // Icon
                        new Image {
                            FlexItem = {
                                Flex = 1f
                            },

                            Sprite = icon,
                            PreserveAspect = true,
                            sColor = color.In()
                        },

                        // Label
                        new Label {
                            Text = text,
                            FontSize = 4.5f,
                            sColor = color.In()
                        }
                    }
                }
            }.WithHoverScale(1f, 1.2f);
        }

        private void Awake() {
            var menuButtonsTheme = _beatLeaderHubTheme.MenuButtonsTheme;

            new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    AlignItems = Align.Center,
                    JustifyContent = Justify.SpaceAround
                },

                Children = {
                    new QuickMiniProfile {
                        FlexItem = {
                            Size = new() { y = 24f, x = 50f }
                        },

                        JustifyContent = Justify.Center
                    }.Bind(ref _quickMiniProfile),

                    // Welcome label
                    new Label {
                            FlexItem = {
                                Size = YogaVector.Auto
                            },

                            Text = "Welcome to the Beat Leader Hub!",
                            FontSize = 6f
                        }
                        //background
                        .InBlurBackground()
                        .AsFlexGroup(padding: new() { left = 2f, top = 1f, right = 2f, bottom = 1f }),

                    // Buttons (use absolute pos for correct overlap)
                    new Layout {
                        FlexController = {
                            JustifyContent = Justify.SpaceAround
                        },

                        FlexItem = {
                            Size = new() { y = 30f, x = 96f }
                        },

                        Children = {
                            CreateActionButton(
                                text: "Battle Royale",
                                icon: BundleLoader.BattleRoyaleIcon,
                                colorSet: menuButtonsTheme.State.Map(static x => x.BattleRoyaleButtonColors),
                                position: YogaFrame.Undefined,
                                iconMargin: 3f,
                                gap: 2f,
                                _battleRoyaleFlowCoordinator,
                                () => _battleRoyaleFlowCoordinator.CanMutateLobby = true
                            ),

                            CreateActionButton(
                                text: "Replay Manager",
                                icon: BundleLoader.ReplayerSettingsIcon,
                                colorSet: menuButtonsTheme.State.Map(static x => x.ReplayManagerButtonColors),
                                position: new() { bottom = 0f, left = 0f },
                                iconMargin: 1f,
                                gap: 1f,
                                _replayManagerFlowCoordinator
                            ),

                            CreateActionButton(
                                text: "Settings",
                                icon: BundleLoader.SettingsIcon,
                                colorSet: menuButtonsTheme.State.Map(static x => x.SettingsButtonColors),
                                position: new() { bottom = 0f, right = 0f },
                                iconMargin: 2.7f,
                                gap: 2f,
                                _settingsFlowCoordinator
                            )
                        }
                    }
                }
            }.WithRectExpand().Use(transform);

            OnInitialize();
        }

        private async void OnInitialize() {
            _quickMiniProfile.SetPlayer(null);
            //waiting for the profile load
            await ProfileManager.WaitUntilProfileLoad();
            var profile = ProfileManager.Profile ?? Player.GuestPlayer;
            _quickMiniProfile.SetPlayer(profile);
        }
    }
}
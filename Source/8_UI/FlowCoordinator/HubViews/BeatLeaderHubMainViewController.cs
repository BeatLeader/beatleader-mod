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
            SerializableColorSet colorSet,
            YogaFrame position,
            float iconMargin,
            float gap,
            FlowCoordinator flowCoordinator,
            Action? beforePresent = null
        ) {
            return new BsAeroButton {
                Skew = 0f,
                sColors = colorSet.State.Map(UIStyle.AeroButtonColors),
                OnClick = () => {
                    beforePresent?.Invoke();

                    _beatLeaderHubFlowCoordinator.PresentFlowCoordinator(
                        flowCoordinator,
                        animationDirection: AnimationDirection.Vertical
                    );
                },
                ConstructContent = (color, _) => new Layout {
                    Children = {
                        // Icon
                        new Image {
                            Sprite = icon,
                            PreserveAspect = true,
                            sColor = color.In()
                        }.AsFlexItem(flex: 1f),

                        // Label
                        new Label {
                            Text = text,
                            FontSize = 4.5f,
                            sColor = color.In()
                        }.AsFlexItem()
                    }
                }.AsFlexGroup(
                    direction: FlexDirection.Column,
                    gap: gap
                ).AsFlexItem(flexGrow: 1f)
            }.WithHoverScale(1f, 1.2f).AsFlexGroup(
                padding: new() { left = 2f, right = 2f, top = iconMargin }
            ).AsFlexItem(size: 30f, position: position);
        }

        private void Awake() {
            var menuButtonsTheme = _beatLeaderHubTheme.MenuButtonsTheme;

            new Layout {
                Children = {
                    new QuickMiniProfile {
                            JustifyContent = Justify.Center
                        }
                        .AsFlexItem(size: new() { y = 24f, x = 50f })
                        .Bind(ref _quickMiniProfile),

                    // Welcome label
                    new Label {
                            Text = "Welcome to the Beat Leader Hub!",
                            FontSize = 6f
                        }
                        .AsFlexItem(size: "auto")
                        //background
                        .InBlurBackground()
                        .AsFlexGroup(padding: new() { left = 2f, top = 1f, right = 2f, bottom = 1f })
                        .AsFlexItem(),

                    // Buttons (use absolute pos for correct overlap)
                    new Layout {
                        Children = {
                            CreateActionButton(
                                text: "Battle Royale",
                                icon: BundleLoader.BattleRoyaleIcon,
                                colorSet: menuButtonsTheme.BattleRoyaleButtonColors,
                                position: YogaFrame.Undefined,
                                iconMargin: 3f,
                                gap: 2f,
                                _battleRoyaleFlowCoordinator,
                                () => _battleRoyaleFlowCoordinator.CanMutateLobby = true
                            ),

                            CreateActionButton(
                                text: "Replay Manager",
                                icon: BundleLoader.ReplayerSettingsIcon,
                                colorSet: menuButtonsTheme.ReplayManagerButtonColors,
                                position: new() { bottom = 0f, left = 0f },
                                iconMargin: 1f,
                                gap: 1f,
                                _replayManagerFlowCoordinator
                            ),

                            CreateActionButton(
                                text: "Settings",
                                icon: BundleLoader.SettingsIcon,
                                colorSet: menuButtonsTheme.SettingsButtonColors,
                                position: new() { bottom = 0f, right = 0f },
                                iconMargin: 2.7f,
                                gap: 2f,
                                _settingsFlowCoordinator
                            )
                        }
                    }.AsFlexGroup(justifyContent: Justify.SpaceAround).AsFlexItem(size: new() { y = 30f, x = 96f })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                alignItems: Align.Center,
                justifyContent: Justify.SpaceAround
            ).WithRectExpand().Use(transform);

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
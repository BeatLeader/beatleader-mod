using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
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

        private void Awake() {
            AeroButtonLayout CreateButton(
                string text,
                Sprite icon,
                IColorSet colorSet,
                FlowCoordinator flowCoordinator,
                Vector2 pivot,
                YogaFrame position,
                float iconMargin,
                float gap
            ) {
                return new AeroButtonLayout {
                    ContentTransform = {
                        pivot = pivot
                    },
                    Colors = colorSet,
                    OnClick = () => {
                        _beatLeaderHubFlowCoordinator.PresentFlowCoordinator(
                            flowCoordinator,
                            animationDirection: AnimationDirection.Vertical
                        );
                    },
                    Children = {
                        //icon
                        new Image {
                            Sprite = icon,
                            PreserveAspect = true
                        }.AsFlexItem(flex: 1f),
                        //label
                        new Label {
                            Text = text,
                            FontSize = 4.5f
                        }.AsFlexItem()
                    }
                }.WithScaleAnimation(1f, 1.2f).AsFlexGroup(
                    direction: FlexDirection.Column,
                    padding: new() { left = 2f, right = 2f, top = iconMargin },
                    gap: gap
                ).AsFlexItem(size: 30f, position: position);
            }

            var menuButtonsTheme = _beatLeaderHubTheme.MenuButtonsTheme;

            new Layout {
                Children = {
                    new QuickMiniProfile {
                            JustifyContent = Justify.Center
                        }
                        .AsFlexItem(size: new() { y = 24f, x = 50f })
                        .Bind(ref _quickMiniProfile),
                    //welcome label
                    new Label {
                            Text = "Welcome to the Beat Leader Hub!",
                            FontSize = 6f
                        }
                        .AsFlexItem(size: "auto")
                        //background
                        .InBlurBackground()
                        .AsFlexGroup(padding: new() { left = 2f, top = 1f, right = 2f, bottom = 1f })
                        .AsFlexItem(),
                    //buttons (use absolute pos for correct overlap)
                    new Layout {
                        Children = {
                            //
                            CreateButton(
                                "Battle Royale",
                                BundleLoader.BattleRoyaleIcon,
                                menuButtonsTheme.BattleRoyaleButtonColors,
                                _battleRoyaleFlowCoordinator,
                                new() { x = 0.5f, y = 0f },
                                YogaFrame.Undefined,
                                3f,
                                2f
                            ),
                            //
                            CreateButton(
                                "Replay Manager",
                                BundleLoader.ReplayerSettingsIcon,
                                menuButtonsTheme.ReplayManagerButtonColors,
                                _replayManagerFlowCoordinator,
                                new() { x = 0f, y = 0f },
                                new() { bottom = 0f, left = 0f },
                                1f,
                                1f
                            ),
                            //
                            CreateButton(
                                "Settings",
                                BundleLoader.SettingsIcon,
                                menuButtonsTheme.SettingsButtonColors,
                                _settingsFlowCoordinator,
                                new() { x = 1f, y = 0f },
                                new() { bottom = 0f, right = 0f },
                                2.7f,
                                2f
                            )
                        }
                    }.AsFlexGroup().AsFlexItem(size: new() { y = 30f, x = 96f })
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                alignItems: Align.Center
            ).WithRectExpand().Use(transform);

            OnInitialize();
        }

        private async void OnInitialize() {
            _quickMiniProfile.SetPlayer(null);
            _quickMiniProfile.JustifyContent = Justify.Center;
            //waiting for the profile load
            await ProfileManager.WaitUntilProfileLoad();
            var profile = ProfileManager.Profile ?? Player.GuestPlayer;
            _quickMiniProfile.SetPlayer(profile);
        }
    }
}
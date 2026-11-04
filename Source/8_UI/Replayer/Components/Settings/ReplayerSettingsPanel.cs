using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using Image = Reactive.BeatSaber.Components.Image;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerSettingsPanel : ReactiveComponent {
        #region Setup

        public void Setup(
            ReplayLaunchData launchData,
            IBeatmapTimeController timeController,
            IReplayFinishController finishController,
            ICameraController cameraController,
            IBodySettingsViewFactory bodySettingsFactory,
            ILayoutEditor? layoutEditor,
            IReplayTimeline timeline,
            IReplayWatermark watermark,
            bool useAlternativeBlur
        ) {
            var settings = launchData.Settings;

            _quickSettingsPanel.Setup(timeController);
            _quickSettingsPanel.IsPresent = settings.UISettings.QuickSettingsEnabled;

            var cameraContr = launchData.DisableBuiltinCamera ? null : cameraController;
            _cameraView.Setup(cameraContr, settings.CameraSettings);

            _uiView.Setup(settings.UISettings, _quickSettingsPanel, layoutEditor, timeline, watermark);
            _otherView.Setup(timeController, finishController);

            var views = _selectorContainer.Views.ToDictionary(static x => x.Key, static x => x.Value);
            views["Avatar"] = bodySettingsFactory.CreateBodySettingsView().AsFlexItem(size: 100.pct);
            _selectorContainer.Views = views;

            RefreshBackgroundBlur(useAlternativeBlur);
        }

        private void RefreshBackgroundBlur(bool useAlternative) {
            _backgroundImage.Material = useAlternative ? BundleLoader.Materials.blurredBackgroundMaterial : GameResources.UIFogBackgroundMaterial;
        }

        #endregion

        #region Construct

        private QuickSettingsPanel _quickSettingsPanel = null!;
        private SettingsCameraView _cameraView = null!;
        private SettingsUIView _uiView = null!;
        private SettingsOtherView _otherView = null!;
        private Image _backgroundImage = null!;

        private KeyedContainer<string> _selectorContainer = null!;
        private State<string> _selectedView = null!;

        protected override GameObject Construct() {
            _selectedView = Remember("Camera");

            return new Layout {
                Children = {
                    //view selector
                    new Layout {
                            FlexController = {
                                FlexDirection = FlexDirection.Column
                            },

                            Children = {
                                CreateViewButton("Camera", BundleLoader.CameraIcon),
                                CreateViewButton("Avatar", BundleLoader.AvatarIcon),
                                CreateViewButton("UI", BundleLoader.UIIcon),
                                CreateViewButton("Other", BundleLoader.OtherIcon)
                            }
                        }
                        .WithRectExpand()
                        .InBackground(
                            sprite: BundleLoader.Sprites.backgroundLeft,
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                        .AsFlexItem(basis: 12f),
                    //view container
                    new Background {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.FlexStart
                        },

                        FlexItem = {
                            Flex = 1f
                        },

                        ContentTransform = {
                            anchorMin = new Vector2(0f, 0f),
                            anchorMax = new Vector2(1f, 0f)
                        },
                        Children = {
                            //actual view
                            new KeyedContainer<string> {
                                    FlexItem = {
                                        FlexGrow = 1f,
                                        Margin = 2f
                                    },

                                    Views = new Dictionary<string, IReactiveComponent> {
                                        ["Camera"] = new SettingsCameraView {
                                            FlexItem = {
                                                Size = 100.pct
                                            },

                                            CameraViewParams = {
                                                new PlayerViewCameraParams().WithRectExpand(),
                                                new FlyingViewCameraParams().WithRectExpand(),
                                                new ManualViewCameraParams().WithRectExpand()
                                            }
                                        }.Bind(ref _cameraView),

                                        ["UI"] = new SettingsUIView {
                                            FlexItem = {
                                                Size = 100.pct
                                            }
                                        }.Bind(ref _uiView),

                                        ["Other"] = new SettingsOtherView {
                                            FlexItem = {
                                                Size = 100.pct
                                            }
                                        }.Bind(ref _otherView)
                                    },
                                    sKey = _selectedView
                                }
                                .AsFlexGroup()
                                .Bind(ref _selectorContainer),

                            //pop-up with quick settings
                            new QuickSettingsPanel().Bind(ref _quickSettingsPanel)
                        }
                    }.AsBackground(
                        sprite: BundleLoader.Sprites.backgroundRight,
                        pixelsPerUnit: 7f
                    ).Bind(ref _backgroundImage)
                }
            }.WithRectExpand().AsFlexGroup().Use();
        }

        protected override void OnInitialize() {
            RefreshBackgroundBlur(false);
        }

        #endregion

        #region View Selector

        [StateGen]
        private Background CreateViewButton(string key, Sprite icon) {
            var state = Remember(GraphicState.None);
            var color = state.MapColorSet(UIStyle.GlowingButtonColorSet);

            _selectedView.AddCallback(x => state.IsActive = x == key);

            return new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        JustifyContent = Justify.Center,
                        Padding = 1f
                    },

                    FlexItem = {
                        Flex = 1f
                    },

                    Color = Color.clear,
                    Children = {
                        new Image {
                                FlexItem = {
                                    Flex = 1f
                                },

                                Sprite = icon,
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                PreserveAspect = true,
                                sColor = color
                            }
                            .In<Layout>()
                            .AsFlexGroup(padding: 1f)
                            .AsFlexItem(
                                minSize: new() { y = 9f },
                                maxSize: new() { y = 10f }
                            ),
                        //
                        new Label {
                            FlexItem = {
                                FlexBasis = 4f
                            },

                            Text = key,
                            Material = BundleLoader.UIAdditiveGlowMaterial,
                            FontSize = 3.6f,
                            sColor = color
                        }
                    }
                }
                .WithPointerEvents(
                    onEnter: _ => state.IsHovered = true,
                    onLeave: _ => state.IsHovered = false,
                    onDown: _ => _selectedView.Value = key
                );
        }

        #endregion
    }
}
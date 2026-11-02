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
            _quickSettingsPanel.SetShown(settings.UISettings.QuickSettingsEnabled, true);

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
                            Children = {
                                CreateViewButton("Camera", BundleLoader.CameraIcon),
                                CreateViewButton("Avatar", BundleLoader.AvatarIcon),
                                CreateViewButton("UI", BundleLoader.UIIcon),
                                CreateViewButton("Other", BundleLoader.OtherIcon)
                            }
                        }
                        .AsFlexGroup(direction: FlexDirection.Column)
                        .WithRectExpand()
                        .InBackground(
                            sprite: BundleLoader.Sprites.backgroundLeft,
                            color: new(0.1f, 0.1f, 0.1f, 1f),
                            pixelsPerUnit: 7f
                        )
                        .AsFlexItem(basis: 12f),
                    //view container
                    new Background {
                        ContentTransform = {
                            anchorMin = new Vector2(0f, 0f),
                            anchorMax = new Vector2(1f, 0f)
                        },
                        Children = {
                            //actual view
                            new KeyedContainer<string> {
                                    Views = new Dictionary<string, IReactiveComponent> {
                                        ["Camera"] = new SettingsCameraView {
                                            CameraViewParams = {
                                                new PlayerViewCameraParams().WithRectExpand(),
                                                new FlyingViewCameraParams().WithRectExpand(),
                                                new ManualViewCameraParams().WithRectExpand()
                                            }
                                        }.AsFlexItem(size: 100.pct).Bind(ref _cameraView),

                                        ["UI"] = new SettingsUIView()
                                            .AsFlexItem(size: 100.pct)
                                            .Bind(ref _uiView),

                                        ["Other"] = new SettingsOtherView()
                                            .AsFlexItem(size: 100.pct)
                                            .Bind(ref _otherView)
                                    },
                                    sKey = _selectedView
                                }
                                .AsFlexItem(flexGrow: 1f, margin: 2f)
                                .AsFlexGroup()
                                .Bind(ref _selectorContainer),

                            //pop-up with quick settings
                            new QuickSettingsPanel().Bind(ref _quickSettingsPanel)
                        }
                    }.AsFlexItem(flex: 1f).AsBackground(
                        sprite: BundleLoader.Sprites.backgroundRight,
                        pixelsPerUnit: 7f
                    ).AsFlexGroup(
                        direction: FlexDirection.Column,
                        justifyContent: Justify.FlexStart
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

            return new Background {
                    Color = Color.clear,
                    Children = {
                        new Image {
                                Sprite = icon,
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                PreserveAspect = true,
                                sColor = color
                            }
                            .AsFlexItem(flex: 1f)
                            .In<Layout>()
                            .AsFlexGroup(padding: 1f)
                            .AsFlexItem(
                                minSize: new() { y = 9f },
                                maxSize: new() { y = 10f }
                            ),
                        //
                        new Label {
                                Text = key,
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                FontSize = 3.6f,
                                sColor = color
                            }
                            .AsFlexItem(basis: 4f)
                    }
                }
                .AsFlexGroup(
                    direction: FlexDirection.Column,
                    justifyContent: Justify.Center,
                    padding: 1f
                )
                .AsFlexItem(flex: 1f)
                .WithPointerEvents(
                    onEnter: _ => state.IsHovered = true,
                    onLeave: _ => state.IsHovered = false,
                    onDown: _ => _selectedView.Value = key
                )
                .On(_selectedView, (_, y) => state.IsActive = y == key);
        }

        #endregion
    }
}
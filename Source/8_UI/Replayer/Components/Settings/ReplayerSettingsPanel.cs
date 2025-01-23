using BeatLeader.Components;
using BeatLeader.Models;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerSettingsPanel : ReactiveComponent {
        #region Setup

        public void Setup(
            ReplayerSettings settings,
            IBeatmapTimeController timeController,
            IReplayFinishController finishController,
            ICameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ILayoutEditor? layoutEditor,
            IReplayTimeline timeline,
            IReplayWatermark watermark,
            bool useAlternativeBlur
        ) {
            _quickSettingsPanel.Setup(timeController);
            _quickSettingsPanel.SetShown(settings.UISettings.QuickSettingsEnabled, true);
            _cameraView.Setup(cameraController, settings.CameraSettings!);
            _avatarView.Setup(bodySpawner, settings.BodySettings);
            _uiView.Setup(settings.UISettings, _quickSettingsPanel, layoutEditor, timeline, watermark);
            _otherView.Setup(timeController, finishController);
            RefreshBackgroundBlur(useAlternativeBlur);
        }

        private void RefreshBackgroundBlur(bool useAlternative) {
            _backgroundImage.Material = useAlternative ? BundleLoader.Materials.blurredBackgroundMaterial : GameResources.UIFogBackgroundMaterial;
        }

        #endregion

        #region Construct

        private QuickSettingsPanel _quickSettingsPanel = null!;
        private SettingsCameraView _cameraView = null!;
        private SettingsAvatarView _avatarView = null!;
        private SettingsUIView _uiView = null!;
        private SettingsOtherView _otherView = null!;
        private Image _backgroundImage = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    //view selector
                    new SegmentedControl<string, Sprite, ViewSegmentedControlCell> {
                        Direction = FlexDirection.Column,
                        Items = {
                            { "Camera", BundleLoader.CameraIcon },
                            { "Avatar", BundleLoader.AvatarIcon },
                            { "UI", BundleLoader.UIIcon },
                            { "Other", BundleLoader.OtherIcon }
                        }
                    }.Export(out var segmentedControl).InBackground(
                        sprite: BundleLoader.Sprites.backgroundLeft,
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexItem(basis: 12f),
                    //view container
                    new Image {
                        ContentTransform = {
                            anchorMin = new Vector2(0f, 0f),
                            anchorMax = new Vector2(1f, 0f)
                        },
                        Children = {
                            //actual view
                            new KeyedContainer<string> {
                                Control = segmentedControl,
                                Items = {
                                    {
                                        "Camera",
                                        new SettingsCameraView {
                                            CameraViewParams = {
                                                new PlayerViewCameraParams(),
                                                new FlyingViewCameraParams(),
                                                new ManualViewCameraParams()
                                            }
                                        }.Bind(ref _cameraView)
                                    },
                                    { "Avatar", new SettingsAvatarView().Bind(ref _avatarView) },
                                    { "UI", new SettingsUIView().Bind(ref _uiView) },
                                    { "Other", new SettingsOtherView().Bind(ref _otherView) }
                                }
                            }.AsFlexItem(grow: 1f, margin: 2f),

                            //pop-up with quick settings
                            new QuickSettingsPanel().Bind(ref _quickSettingsPanel)
                        }
                    }.AsFlexItem(grow: 1f).AsBackground(
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

        #region ViewSegmentedControl

        private class ViewSegmentedControlCell : KeyedControlComponentCell<string, Sprite> {
            #region Setup

            private SegmentedControlButton _button = null!;

            public override void OnInit(string name, Sprite icon) {
                _button.Icon = icon;
                _button.Text = name;
            }

            protected override GameObject Construct() {
                return new SegmentedControlButton {
                    Colors = UIStyle.ButtonColorSet,
                    Latching = true,
                    OnStateChanged = HandleButtonStateChanged
                }.WithRectExpand().Bind(ref _button).Use();
            }

            protected override void OnStart() {
                ContentTransform.parent.localScale = Vector3.one;
            }

            #endregion

            #region Button

            private class SegmentedControlButton : ColoredButton {
                #region Setup

                public string Text {
                    get => _text.Text;
                    set => _text.Text = value;
                }

                public Sprite? Icon {
                    get => _icon.Sprite;
                    set => _icon.Sprite = value;
                }

                #endregion

                #region Construct

                private Image _icon = null!;
                private Label _text = null!;

                protected override GameObject Construct() {
                    var dummy = new Dummy {
                        Children = {
                            new Image {
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                PreserveAspect = true
                            }.AsFlexItem(grow: 1f).Bind(ref _icon).In<Dummy>().AsFlexGroup(
                                padding: 1f
                            ).AsFlexItem(aspectRatio: 1f),
                            //
                            new Label {
                                Material = BundleLoader.UIAdditiveGlowMaterial,
                                FontSize = 3.6f
                            }.AsFlexItem(basis: 4f).Bind(ref _text)
                        }
                    }.AsFlexGroup(
                        direction: FlexDirection.Column,
                        justifyContent: Justify.Center,
                        padding: 1f
                    );
                    Construct(dummy.ContentTransform);
                    return dummy.Use();
                }

                #endregion

                #region ApplyColor

                protected override void ApplyColor(Color color) {
                    _icon.Color = color;
                    _text.Color = color;
                }

                #endregion
            }

            #endregion

            #region Callbacks

            public override void OnCellStateChange(bool selected) {
                _button.Click(selected);
            }

            private void HandleButtonStateChanged(bool state) {
                if (!state) {
                    _button.Click(true);
                }
                SelectSelf();
            }

            #endregion
        }

        #endregion
    }
}
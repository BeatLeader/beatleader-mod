using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;
using FlexDirection = BeatLeader.UI.Reactive.Yoga.FlexDirection;

namespace BeatLeader.UI.Replayer {
    internal class ReplayerSettingsPanel : ReactiveComponent {
        #region Setup

        public void Setup(
            ReplayerSettings settings,
            IBeatmapTimeController timeController,
            ICameraController cameraController,
            IVirtualPlayerBodySpawner bodySpawner,
            ILayoutEditor? layoutEditor,
            IReplayTimeline timeline,
            IReplayWatermark watermark,
            bool useAlternativeBlur
        ) {
            _cameraView.Setup(cameraController, settings.CameraSettings!);
            _avatarView.Setup(bodySpawner, settings.BodySettings);
            _uiView.Setup(timeController, layoutEditor, timeline, watermark);
            RefreshBackgroundBlur(useAlternativeBlur);
        }

        private void RefreshBackgroundBlur(bool useAlternative) {
            _backgroundImage.Material = useAlternative ? 
                BundleLoader.Materials.uiBlurMaterial :
                GameResources.UIFogBackgroundMaterial;
        }
        
        #endregion

        #region Construct

        private SettingsCameraView _cameraView = null!;
        private SettingsAvatarView _avatarView = null!;
        private SettingsUIView _uiView = null!;
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
                            { "Other", BundleLoader.OtherIcon }
                        }
                    }.Export(out var segmentedControl).InBackground(
                        sprite: BundleLoader.Sprites.backgroundLeft,
                        color: new(0.1f, 0.1f, 0.1f, 1f),
                        pixelsPerUnit: 7f
                    ).AsFlexItem(basis: 12f),
                    //view container
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
                            { "Other", new SettingsUIView().Bind(ref _uiView) }
                        }
                    }.AsFlexItem(grow: 1f).InBackground(
                        sprite: BundleLoader.Sprites.backgroundRight,
                        pixelsPerUnit: 7f
                    ).AsFlexGroup(padding: 2f).AsFlexItem(grow: 1f).Bind(ref _backgroundImage),
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
                    Sticky = true
                }.WithRectExpand().WithStateListener(HandleButtonStateChanged).Bind(ref _button).Use();
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
using System;
using BeatLeader.Installers;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.UI.Hub {
    internal class EditAvatarFloatingButton : ReactiveComponent {
        #region Setup

        public bool Interactable {
            get => _button.Interactable;
            set => _button.Interactable = value;
        }

        public BsAeroButtonColors Colors {
            get => _button.Colors;
            set => _button.Colors = value;
        }

        public Action? OnClick;

        #endregion

        #region Construct

        private BsAeroButton _button = null!;
        private AnimatedState<float> _alpha = null!;

        protected override GameObject Construct() {
            _alpha = RememberAnimated(0f, 10.fact);

            return new BsAeroButton {
                FlexController = {
                    JustifyContent = Justify.Center,
                    Padding = 2f,
                    ConstrainHorizontal = false
                },

                FlexItem = {
                    Size = new() { y = 14f }
                },

                OnClick = () => OnClick?.Invoke(),
                ConstructContent = (color, skew) => new Layout {
                    FlexController = {
                        Gap = 2f
                    },

                    Children = {
                        new Image {
                            FlexItem = {
                                AspectRatio = 1f
                            },

                            Sprite = BundleLoader.Sprites.editIcon,
                            sColor = color.In(),
                            sSkew = skew.In()
                        },
                        //
                        new Label {
                            FlexItem = {
                                Size = new() { x = "auto" }
                            },

                            Text = "Edit Avatar",
                            FontSize = 6f,
                            sColor = color.In(),
                            sFontStyle = skew.Map(x => x > 0f ? FontStyles.Italic : FontStyles.Normal)
                        }
                    }
                }
            }.WithHoverScale(0.02f, 0.024f).WithAlpha(_alpha).Bind(ref _button).Use();
        }

        protected override void OnInitialize() {
            BeatSaberUtils.AddCanvas(this);
            var raycaster = Content.AddComponent<VRGraphicRaycaster>();
            OnMenuInstaller.Container.Inject(raycaster);
        }

        #endregion

        #region Animation

        public bool IsPresent {
            get => Enabled;
            set {
                Enabled = value;

                if (value) {
                    _alpha.SetValueImmediate(0f);
                    _alpha.TargetValue = 1f;
                }
            }
        }

        #endregion
    }
}
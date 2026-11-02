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
                OnClick = () => OnClick?.Invoke(),
                ConstructContent = (color, skew) => new Layout {
                    Children = {
                        new Image {
                            Sprite = BundleLoader.Sprites.editIcon,
                            sColor = color.In(),
                            sSkew = skew.In()
                        }.AsFlexItem(aspectRatio: 1f),
                        //
                        new Label {
                            Text = "Edit Avatar",
                            FontSize = 6f,
                            sColor = color.In(),
                            sFontStyle = skew.Map(x => x > 0f ? FontStyles.Italic : FontStyles.Normal)
                        }.AsFlexItem(size: new() { x = "auto" })
                    }
                }.AsFlexGroup(gap: 2f)
            }.WithHoverScale(0.02f, 0.024f).WithAlpha(_alpha).AsFlexGroup(
                justifyContent: Justify.Center,
                padding: 2f,
                constrainHorizontal: false
            ).AsFlexItem(size: new() { y = 14f }).Bind(ref _button).Use();
        }

        protected override void OnInitialize() {
            BeatSaberUtils.AddCanvas(this);
            var raycaster = Content.AddComponent<VRGraphicRaycaster>();
            OnMenuInstaller.Container.Inject(raycaster);
        }

        #endregion

        #region Animation

        public void Present() {
            Enabled = true;
            _alpha.SetValueImmediate(0f);
            _alpha.TargetValue = 1f;
        }

        public void Hide() {
            Enabled = false;
        }

        #endregion
    }
}
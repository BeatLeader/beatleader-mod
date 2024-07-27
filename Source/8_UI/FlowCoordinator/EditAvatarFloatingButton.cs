using System;
using BeatLeader.Installers;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;
using VRUIControls;

namespace BeatLeader.UI.Hub {
    internal class EditAvatarFloatingButton : ReactiveComponent, IClickableComponent {
        #region Setup

        public bool Interactable {
            get => _button.Interactable;
            set => _button.Interactable = value;
        }

        public event Action? ClickEvent;

        public void Setup(BeatLeaderHubMenuButtonsTheme theme) {
            _button.Colors = theme.EditAvatarButtonColors;
        }

        #endregion

        #region Construct

        private AeroButton _button = null!;
        private CanvasGroup _canvasGroup = null!;

        protected override GameObject Construct() {
            return new AeroButton {
                Children = {
                    new Image {
                        Sprite = BundleLoader.Sprites.editIcon
                    }.AsFlexItem(aspectRatio: 1f),
                    //
                    new Label {
                        Text = "Edit Avatar",
                        FontSize = 6f
                    }.AsFlexItem(size: new() { x = "auto" })
                },
                BaseScale = 0.02f * Vector3.one,
                HoverScaleSum = 0.004f * Vector3.one
            }.WithNativeComponent(out _canvasGroup).WithClickListener(() => ClickEvent?.Invoke()).AsFlexGroup(
                justifyContent: Justify.Center,
                padding: 2f,
                gap: 2f
            ).WithSizeDelta(40f, 14f).Bind(ref _button).Use();
        }

        protected override void OnInitialize() {
            ReactiveUtils.AddCanvas(this);
            var raycaster = Content.AddComponent<VRGraphicRaycaster>();
            OnMenuInstaller.Container.Inject(raycaster);
        }

        #endregion

        #region Animation

        private readonly ValueAnimator _valueAnimator = new();

        public void Present() {
            Enabled = true;
            _valueAnimator.SetProgress(0f);
            _valueAnimator.SetTarget(1f);
            OnUpdate();
        }

        public void Hide() {
            Enabled = false;
        }

        protected override void OnUpdate() {
            _valueAnimator.Update();
            _canvasGroup.alpha = _valueAnimator.Progress;
        }

        #endregion
    }
}
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal record DynamicShadowSettings(
        float Size = 10f,
        Vector2 Offset = default
    );

    internal class DynamicShadow : ReactiveComponent {
        #region Setup

        public DynamicShadowSettings Settings {
            get;
            set {
                field = value;
                RefreshRect();
            }
        } = new();

        private void RefreshRect() {
            var halfSize = Settings.Size / 2f * Vector2.one;

            ContentTransform.offsetMin = Settings.Offset - halfSize;
            ContentTransform.offsetMax = Settings.Offset + halfSize;
        }

        #endregion

        #region Construct

        private CanvasGroup _canvasGroup = null!;

        protected override GameObject Construct() {
            return new Image {
                IgnoreLayout = true,
                RaycastTarget = false,
                Sprite = BundleLoader.Sprites.glare,
                Color = Color.black.ColorWithAlpha(0.7f),
                ImageType = UnityEngine.UI.Image.Type.Sliced
            }.WithRectExpand().WithNativeComponent(out _canvasGroup).Use();
        }

        protected override void OnInitialize() {
            RefreshRect();
        }

        #endregion

        #region Animation

        protected override void OnEnable() {
            StopAllCoroutines();
            StartCoroutine(
                _canvasGroup.AnimateGroupCoroutine(
                    0f,
                    1f,
                    duration: 0.1f,
                    startImmediately: true
                )
            );
        }

        #endregion
    }
}
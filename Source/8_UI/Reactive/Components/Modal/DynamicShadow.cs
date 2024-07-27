using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal record DynamicShadowSettings(
        float Size = 10f,
        Vector2 Offset = default
    );

    internal class DynamicShadow : ReactiveComponent {
        #region Pool

        private static readonly ReactivePool<DynamicShadow> shadowsPool = new();

        public static DynamicShadow SpawnShadow(RectTransform transform, DynamicShadowSettings settings) {
            var shadow = shadowsPool.Spawn();
            shadow.Setup(transform, settings);
            return shadow;
        }

        public static void DespawnShadow(DynamicShadow shadow) {
            shadowsPool.Despawn(shadow);
        }

        #endregion

        #region Setup

        public void Setup(RectTransform transform, DynamicShadowSettings settings) {
            ContentTransform.SetParent(transform.parent, false);
            ContentTransform.SetSiblingIndex(transform.GetSiblingIndex());
            var size = transform.rect.size;
            var posSum = (transform.pivot * 2 - Vector2.one) * transform.rect.size / 2;
            ContentTransform.sizeDelta = size + settings.Size * Vector2.one;
            ContentTransform.localPosition = (Vector2)transform.localPosition - posSum + settings.Offset;
        }

        #endregion

        #region Construct

        private Image _image = null!;
        private CanvasGroup _canvasGroup = null!;

        protected override GameObject Construct() {
            return new Image {
                Sprite = BundleLoader.Sprites.glare,
                Color = Color.black.ColorWithAlpha(0.7f),
                ImageType = UnityEngine.UI.Image.Type.Sliced
            }.WithNativeComponent(out _canvasGroup).Bind(ref _image).Use();
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
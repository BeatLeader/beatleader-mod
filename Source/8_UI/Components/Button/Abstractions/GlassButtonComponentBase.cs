using BeatLeader.Utils;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class GlassButtonComponentBase<T> : ColoredButtonComponentBase<T> where T : ReeUIComponentV3<T> {
        #region Color

        public static readonly Color HoveredGradientColor = Color.white.ColorWithAlpha(0.5f);
        public static readonly Color DefaultGradientColor = Color.white;

        protected override void ApplyColor(Color color, float t) {
            var gradientColor1 = LerpColor(t, DefaultGradientColor, HoveredGradientColor);
            _backgroundImage.color = color;
            _backgroundImage.color1 = gradientColor1;
        }

        protected override void OnHoverProgressChange(float progress) {
            UpdateColor(progress);
        }

        protected override void OnButtonStateChange(bool state) {
            UpdateColor(1);
        }

        #endregion

        #region Interactable

        public static readonly Color DisabledColor = Color.white.ColorWithAlpha(0.2f);

        protected override void OnInteractableChange(bool interactable) {
            _backgroundImage.gradient = interactable;
            if (interactable) {
                UpdateColor(0f);
            } else {
                _backgroundImage.color = DisabledColor;
            }
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            HoverColor = new(0f, 0.75f, 1f, 1f);
            Color = Color.black.ColorWithAlpha(0.5f);
        }

        #endregion

        #region Construct

        protected ImageView GlassBackgroundImage => _backgroundImage;
        protected LayoutGroup GlassBackgroundGroup => _backgroundGroup;

        private ImageView _backgroundImage = null!;
        private HorizontalOrVerticalLayoutGroup _backgroundGroup = null!;

        protected sealed override void OnContentConstruct(Transform parent) {
            var backgroundGo = parent.gameObject.CreateChild("Background");
            var backgroundTransform = backgroundGo.AddComponent<RectTransform>();
            backgroundTransform.anchorMin = Vector2.zero;
            backgroundTransform.anchorMax = Vector2.one;
            backgroundTransform.sizeDelta = Vector2.zero;

            _backgroundImage = backgroundGo.AddComponent<AdvancedImageView>();
            _backgroundImage.sprite = BundleLoader.WhiteBG;
            _backgroundImage.color = DefaultColor;
            _backgroundImage.color0 = DefaultGradientColor;
            _backgroundImage.color1 = DefaultGradientColor;
            _backgroundImage.gradient = true;
            _backgroundImage.type = Image.Type.Sliced;
            _backgroundImage.pixelsPerUnitMultiplier = 10;
            _backgroundImage.material = GameResources.UINoGlowMaterial;
            _backgroundImage.SetField("_gradientDirection", ImageView.GradientDirection.Vertical);

            _backgroundGroup = backgroundGo.AddComponent<HorizontalLayoutGroup>();
            _backgroundGroup.childControlHeight = true;
            _backgroundGroup.childControlWidth = true;
            _backgroundGroup.childForceExpandHeight = true;
            _backgroundGroup.childForceExpandWidth = true;

            OnGlassButtonContentConstruct(backgroundTransform);
        }

        protected virtual void OnGlassButtonContentConstruct(Transform parent) { }

        #endregion
    }
}
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.AspectRatioFitter;

namespace BeatLeader.Components {
    internal class ModalButton : ButtonComponentBase<ModalButton> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public RectOffset ContentPad {
            get => _backgroundGroup.padding;
            set => _backgroundGroup.padding = value;
        }

        [ExternalProperty, UsedImplicitly]
        public FlexDirection ContentDirection {
            get => _flexGroup.FlexDirection;
            set {
                _flexGroup.FlexDirection = value;
                var row = value is FlexDirection.Row or FlexDirection.RowReverse;
                _text.alignment = row ?
                    TextAlignmentOptions.MidlineLeft :
                    TextAlignmentOptions.Center;
                _iconAspectFitter.aspectMode = row ?
                    AspectMode.HeightControlsWidth :
                    AspectMode.WidthControlsHeight;
                _iconFlexItem.FlexGrow = row ? 0 : 1;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public JustifyContent ContentAlign {
            get => _flexGroup.JustifyContent;
            set {
                var mustFitContent = value is JustifyContent.Center;
                _textGroup.enabled = mustFitContent;
                _textFlexItem.FlexGrow = mustFitContent ? 0 : 1;
                _flexGroup.JustifyContent = value;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool ColorizeOnHover {
            get => _colorizeOnHover;
            set {
                _colorizeOnHover = value;
                if (!IsInitialized) return;
                UpdateColor(0);
            }
        }

        [ExternalProperty, UsedImplicitly]
        public Color HoverColor { get; set; } = HoveredColor;

        [ExternalProperty, UsedImplicitly]
        public Color Color {
            get => _color;
            set {
                _color = value;
                UpdateColor(0);
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool ShowText {
            get => _textGameObject.activeInHierarchy;
            set => _textGameObject.SetActive(value);
        }

        [ExternalProperty, UsedImplicitly]
        public bool ShowIcon {
            get => _iconGameObject.activeInHierarchy;
            set => _iconGameObject.SetActive(value);
        }

        [ExternalProperty, UsedImplicitly]
        public bool ExpandIcon {
            get => _iconAspectFitter.enabled;
            set => _iconAspectFitter.enabled = value;
        }

        [ExternalProperty, UsedImplicitly]
        public string Text {
            get => _text.text;
            set => _text.text = value;
        }

        private bool _colorizeOnHover = true;
        private Color _color = DefaultColor;

        #endregion

        #region UI Components

        [UsedImplicitly]
        [ExternalProperty(
            prefix: "icon",
            nameof(AdvancedImage.Sprite),
            nameof(AdvancedImage.Pad)
        )]
        public AdvancedImage Icon { get; private set; } = null!;

        private TMP_Text _text = null!;
        private FlexItem _textFlexItem = null!;
        private LayoutGroup _textGroup = null!;
        private GameObject _textGameObject = null!;

        private GameObject _iconGameObject = null!;
        private FlexItem _iconFlexItem = null!;
        private AspectRatioFitter _iconAspectFitter = null!;

        private ImageView _backgroundImage = null!;
        private HorizontalOrVerticalLayoutGroup _backgroundGroup = null!;
        private FlexContainer _flexGroup = null!;

        #endregion

        #region Color

        public static readonly Color HoveredGradientColor = Color.white.ColorWithAlpha(0.5f);
        public static readonly Color DefaultGradientColor = Color.white;
        public static readonly Color HoveredColor = new(0f, 0.75f, 1f, 1f);
        public static readonly Color DefaultColor = Color.black.ColorWithAlpha(0.5f);

        private void UpdateColor(float hoverProgress) {
            hoverProgress = _colorizeOnHover ? hoverProgress : 0;
            _backgroundImage.color = IsActive ? HoverColor : Color.Lerp(Color, HoverColor, hoverProgress);
            _backgroundImage.color1 = IsActive ? HoveredGradientColor : Color.Lerp(DefaultGradientColor, HoveredGradientColor, hoverProgress);
        }

        protected override void OnHoverProgressChange(float progress) {
            UpdateColor(progress);
        }

        protected override void OnButtonStateChange(bool state) {
            UpdateColor(1);
        }

        #endregion

        #region Construction

        private Transform CreateBackground(Transform parent) {
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

            return backgroundTransform;
        }

        private Transform CreateContent(Transform parent) {
            var contentGo = parent.gameObject.CreateChild("Content");
            _flexGroup = contentGo.AddComponent<FlexContainer>();
            _flexGroup.FlexDirection = FlexDirection.Row;
            _flexGroup.AlignItems = AlignItems.Stretch;
            _flexGroup.Gap = Vector2.one;
            return contentGo.transform;
        }

        private void CreateIcon(Transform parent) {
            Icon = AdvancedImage.Instantiate(parent);
            _iconGameObject = Icon.Content;
            _iconGameObject.GetComponent<ContentSizeFitter>().enabled = false;
            _iconAspectFitter = _iconGameObject.AddComponent<AspectRatioFitter>();
            _iconAspectFitter.aspectRatio = 1;
            _iconAspectFitter.aspectMode = AspectMode.HeightControlsWidth;
            _iconFlexItem = _iconGameObject.AddComponent<FlexItem>();
            _iconFlexItem.MinSize = new Vector2(4, -1);
        }

        private void CreateText(Transform parent) {
            _textGameObject = parent.gameObject.CreateChild("Text");
            _text = _textGameObject.AddComponent<CurvedTextMeshPro>();
            _text.font = BeatSaberUI.MainTextFont;
            _text.fontSharedMaterial = GameResources.UIFontMaterial;
            _text.fontSize = 4f;
            _text.alignment = TextAlignmentOptions.MidlineLeft;
            _text.enableWordWrapping = false;

            _textGroup = _textGameObject.AddComponent<HorizontalLayoutGroup>();
            _textFlexItem = _textGameObject.AddComponent<FlexItem>();
            _textFlexItem.FlexGrow = 1;
        }

        protected override void OnContentConstruct(Transform parent) {
            var backgroundGo = CreateBackground(parent);
            var contentGo = CreateContent(backgroundGo);
            CreateIcon(contentGo);
            CreateText(contentGo);
        }

        protected override void OnInitializeInternal() {
            GrowOnHover = false;
            HoverLerpMul = 10000;
            ContentAlign = JustifyContent.FlexStart;
            ContentPad = new RectOffset(1, 1, 1, 1);
        }

        #endregion
    }
}
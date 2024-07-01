using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.AspectRatioFitter;

namespace BeatLeader.Components {
    internal class ModalButton : GlassButtonComponentBase<ModalButton> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public RectOffset ContentPad {
            get => GlassBackgroundGroup.padding;
            set => GlassBackgroundGroup.padding = value;
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

        private FlexContainer _flexGroup = null!;

        #endregion

        #region Construction

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

        protected override void OnGlassButtonContentConstruct(Transform parent) {
            var contentGo = CreateContent(parent);
            CreateIcon(contentGo);
            CreateText(contentGo);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            GrowOnHover = false;
            HoverLerpMul = 10000;
            ContentAlign = JustifyContent.FlexStart;
            ContentPad = new RectOffset(1, 1, 1, 1);
        }

        #endregion
    }
}
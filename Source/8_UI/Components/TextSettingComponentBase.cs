using BeatLeader.Utils;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class TextSettingComponentBase<T> : LayoutComponentBase<T> where T : ReeUIComponentV3<T> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public bool ShowText {
            get => _placeholderTextGo.activeSelf;
            set => _placeholderTextGo.SetActive(value);
        }

        [ExternalProperty, UsedImplicitly]
        public string Text {
            get => _placeholderText.text;
            set => _placeholderText.text = value;
        }

        [ExternalProperty, UsedImplicitly]
        public float SettingWidth {
            get => SettingLayoutElement.preferredWidth;
            set => SettingLayoutElement.preferredWidth = value;
        }

        [ExternalProperty, UsedImplicitly]
        public abstract bool Interactable { get; set; }

        #endregion

        #region Construct

        protected override LayoutGroupType LayoutGroupDirection => LayoutGroupType.Horizontal;

        protected LayoutElement SettingLayoutElement { get; private set; } = null!;
        protected RectTransform SettingTransform { get; private set; } = null!;

        private GameObject _placeholderTextGo = null!;
        private TMP_Text _placeholderText = null!;

        private void CreateSetting(Transform parent) {
            var rect = ConstructSetting(parent);
            var layoutElement = rect.gameObject.GetOrAddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;
            SettingTransform = rect;
            SettingLayoutElement = layoutElement;
            Interactable = true;
        }

        private void CreateText(Transform parent) {
            _placeholderTextGo = parent.gameObject.CreateChild("Text");
            var layoutElement = _placeholderTextGo.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1;

            _placeholderText = _placeholderTextGo.AddComponent<TextMeshProUGUI>();
            _placeholderText.alignment = TextAlignmentOptions.Left;
            _placeholderText.fontSize = 4f;
            _placeholderText.text = "Default Text";
            _placeholderText.overflowMode = TextOverflowModes.Ellipsis;
        }

        protected sealed override void OnConstruct(Transform parent) {
            CreateText(parent);
            CreateSetting(parent);
        }

        protected override void OnInitialize() {
            Height = 7;
            InheritWidth = true;
        }

        protected abstract RectTransform ConstructSetting(Transform parent);

        #endregion
    }
}
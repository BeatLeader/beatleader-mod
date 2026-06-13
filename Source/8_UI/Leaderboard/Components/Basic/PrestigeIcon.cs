using BeatLeader.DataManager;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class PrestigeIcon : ReeUIComponentV2 {
        #region Events

        protected override void OnInitialize() {
            _image.material = BundleLoader.PrestigeIconMaterial;
        }

        private void OnEnable() {
            PrestigeLevelsManager.IconsLoadedEvent += OnIconsLoaded;
            UpdateImage();
        }

        private void OnDisable() {
            PrestigeLevelsManager.IconsLoadedEvent -= OnIconsLoaded;
            StopAllCoroutines();
        }

        private void OnIconsLoaded() {
            UpdateImage();
        }

        #endregion

        #region SetPrestige

        private int _prestige = 0;

        public void SetPrestige(int prestige) {
            _prestige = prestige;
            if (gameObject.activeInHierarchy) UpdateImage();
        }

        public void SetActive(bool active) {
            Content.gameObject.SetActive(active);
        }

        #endregion

        #region Image

        public int Size {
            set {
                _layoutElement.preferredHeight = value;
                _layoutElement.preferredWidth = value;
            }
        }

        [UIComponent("image-component"), UsedImplicitly]
        private Image _image = null!;

        [UIComponent("image-component"), UsedImplicitly]
        private LayoutElement _layoutElement = null!;

        public void SetAlpha(float value) {
            _image.color = new Color(1, 1, 1, value);
        }

        private void UpdateImage() {
            if (_image != null) {
                _image.sprite = PrestigeLevelsManager.GetIcon(_prestige);
            }
        }

        #endregion
    }
}
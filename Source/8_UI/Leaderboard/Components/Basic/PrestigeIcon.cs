using BeatLeader.DataManager;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class PrestigeIcon : ReeUIComponentV2 {
        #region Events

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

        /// <summary>
        /// Gets the small prestige icon sprite for score rows
        /// </summary>
        public static Sprite GetPrestigeSprite(int prestige) {
            return PrestigeLevelsManager.GetSmallIcon(prestige);
        }

        /// <summary>
        /// Gets the big prestige icon sprite for player info
        /// </summary>
        public static Sprite GetBigPrestigeSprite(int prestige) {
            return PrestigeLevelsManager.GetBigIcon(prestige);
        }

        private void UpdateImage() {
            SetSprite(GetPrestigeSprite(_prestige));
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private Image _image;

        public void SetAlpha(float value) {
            _image.color = new Color(1, 1, 1, value);
        }

        private void SetSprite(Sprite sprite) {
            if (_image != null) _image.sprite = sprite;
        }

        #endregion
    }
}

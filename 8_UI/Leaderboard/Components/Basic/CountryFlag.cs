using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class CountryFlag : ReeUIComponentV2 {
        #region Events

        private void OnEnable() {
            if (_country != null) UpdateImage();
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        #endregion

        #region SetCountry

        private string _country;

        public void SetCountry(string country) {
            _country = country;
            if (gameObject.activeInHierarchy) UpdateImage();
        }

        private void UpdateImage() {
            SetSprite(BundleLoader.GetSpriteFromBundle(_country.ToLower()) ?? BundleLoader.LocationIcon);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private Image _image;

        public void SetAlpha(float value) {
            _image.color = new Color(1, 1, 1, value);
        }

        private void SetSprite(Sprite sprite) {
            _image.sprite = sprite;
        }

        #endregion
    }
}
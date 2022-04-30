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
            Clear();
            var loadTask = CountryFlagsStorage.GetCountryFlagCoroutine(_country, false, OnLoadSuccess, OnLoadFailed);
            StartCoroutine(loadTask);
        }

        private void Clear() {
            SetSprite(BundleLoader.TransparentPixel);
        }

        private void OnLoadSuccess(Sprite sprite) {
            SetSprite(sprite);
        }

        private void OnLoadFailed(string reason) {
            SetSprite(BundleLoader.LocationIcon);
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly]
        private Image _image;

        private void SetSprite(Sprite sprite) {
            _image.sprite = sprite;
        }

        #endregion
    }
}
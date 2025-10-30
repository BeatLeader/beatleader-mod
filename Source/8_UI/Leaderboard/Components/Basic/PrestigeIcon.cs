using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class PrestigeIcon : ReeUIComponentV2 {
        #region Events

        private void OnEnable() {
            UpdateImage();
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        #endregion

        #region SetCountry

        private int _prestige = 0;

        public void SetPrestige(int prestige) {
            _prestige = prestige;
            if (gameObject.activeInHierarchy) UpdateImage();
        }

        public static Sprite GetPrestigeSprite(int prestige) {
            switch (prestige) {
                case 0:
                    return BundleLoader.PrestigeIcon0 ?? BundleLoader.LocationIcon;
                case 1:
                    return BundleLoader.PrestigeIcon1 ?? BundleLoader.LocationIcon;
                case 2:
                    return BundleLoader.PrestigeIcon2 ?? BundleLoader.LocationIcon;
                case 3:
                    return BundleLoader.PrestigeIcon3 ?? BundleLoader.LocationIcon;
                case 4:
                    return BundleLoader.PrestigeIcon4 ?? BundleLoader.LocationIcon;
                case 5:
                    return BundleLoader.PrestigeIcon5 ?? BundleLoader.LocationIcon;
                case 6:
                    return BundleLoader.PrestigeIcon6 ?? BundleLoader.LocationIcon;
                case 7:
                    return BundleLoader.PrestigeIcon7 ?? BundleLoader.LocationIcon;
                case 8:
                    return BundleLoader.PrestigeIcon8 ?? BundleLoader.LocationIcon;
                case 9:
                    return BundleLoader.PrestigeIcon9 ?? BundleLoader.LocationIcon;
                case 10:
                    return BundleLoader.PrestigeIcon10 ?? BundleLoader.LocationIcon;
                default:
                    return BundleLoader.LocationIcon;
            }
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
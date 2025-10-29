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

        private int? _prestige = 0;

        public void SetPrestige(int? prestige) {
            _prestige = prestige;
            if (gameObject.activeInHierarchy) UpdateImage();
        }

        private void UpdateImage() {
            switch (_prestige) {
                case 0:
                    SetSprite(BundleLoader.PrestigeIcon0 ?? BundleLoader.LocationIcon);
                    break;
                case 1:
                    SetSprite(BundleLoader.PrestigeIcon1 ?? BundleLoader.LocationIcon);
                    break;
                case 2:
                    SetSprite(BundleLoader.PrestigeIcon2 ?? BundleLoader.LocationIcon);
                    break;
                case 3:
                    SetSprite(BundleLoader.PrestigeIcon3 ?? BundleLoader.LocationIcon);
                    break;
                case 4:
                    SetSprite(BundleLoader.PrestigeIcon4 ?? BundleLoader.LocationIcon);
                    break;
                case 5:
                    SetSprite(BundleLoader.PrestigeIcon5 ?? BundleLoader.LocationIcon);
                    break;
                case 6:
                    SetSprite(BundleLoader.PrestigeIcon6 ?? BundleLoader.LocationIcon);
                    break;
                case 7:
                    SetSprite(BundleLoader.PrestigeIcon7 ?? BundleLoader.LocationIcon);
                    break;
                case 8:
                    SetSprite(BundleLoader.PrestigeIcon8 ?? BundleLoader.LocationIcon);
                    break;
                case 9:
                    SetSprite(BundleLoader.PrestigeIcon9 ?? BundleLoader.LocationIcon);
                    break;
                case 10:
                    SetSprite(BundleLoader.PrestigeIcon10 ?? BundleLoader.LocationIcon);
                    break;
            }
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
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class FeaturedPreviewPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("image"), UsedImplicitly]
        private ImageView _image = null!;

        [UIComponent("top-text"), UsedImplicitly]
        private TMP_Text _topText = null!;

        [UIComponent("bottom-text"), UsedImplicitly]
        private TMP_Text _bottomText = null!;

        #endregion

        #region Setup

        public async void Setup(string previewUrl, string topText, string bottomText) {
            _topText.text = $" {topText}";
            _bottomText.text = bottomText;
            if (!string.IsNullOrEmpty(previewUrl)) {
                await _image.SetImageAsync(previewUrl);
            }
        }

        protected override void OnInitialize() {
            _image.material = BundleLoader.RoundTexture10Material;
            _image._skew = 0.18f;
            _image.__Refresh();
            
            _topText.overflowMode = TextOverflowModes.Ellipsis;
            _bottomText.overflowMode = TextOverflowModes.Ellipsis;
        }

        #endregion
    }
}
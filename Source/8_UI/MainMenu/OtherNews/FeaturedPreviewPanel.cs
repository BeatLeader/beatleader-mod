using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class FeaturedPreviewPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("image"), UsedImplicitly] private ImageView _image = null!;

        [UIComponent("top-text"), UsedImplicitly] private TMP_Text _topText = null!;

        [UIComponent("bottom-text"), UsedImplicitly] private TMP_Text _bottomText = null!;

        [UIComponent("button"), UsedImplicitly] private TMP_Text _buttonText = null!;

        [UIComponent("button"), UsedImplicitly] private Button _button = null!;

        #endregion

        #region Setup

        private Action? _buttonAction;

        public async void Setup(string previewUrl, string topText, string bottomText, string buttonText, Action buttonAction) {
            _topText.text = $" {topText}";
            _bottomText.text = bottomText;
            _buttonText.text = buttonText;
            _buttonAction = buttonAction;
            if (!string.IsNullOrEmpty(previewUrl)) {
                await _image.SetImageAsync(previewUrl);
            }
        }

        protected override void OnInitialize() {
            _image.material = BundleLoader.RoundTextureMaterial;
            _image._skew = 0.18f;
            _image.__Refresh();

            _topText.overflowMode = TextOverflowModes.Ellipsis;
            _bottomText.overflowMode = TextOverflowModes.Ellipsis;
        }

        [UIAction("OnButtonPressed"), UsedImplicitly]
        private void OnButtonPressed() {
            _buttonAction?.Invoke();
        }

        #endregion
    }
}
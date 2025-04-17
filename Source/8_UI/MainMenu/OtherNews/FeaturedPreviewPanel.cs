using System;
using BeatLeader.Components;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.MainMenu {
    internal class FeaturedPreviewPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("background"), UsedImplicitly] private ImageView _background = null!;
        
        [UIComponent("image"), UsedImplicitly] private ImageView _image = null!;

        [UIComponent("top-text"), UsedImplicitly] private TMP_Text _topText = null!;

        [UIComponent("bottom-text"), UsedImplicitly] private TMP_Text _bottomText = null!;

        [UIComponent("button"), UsedImplicitly] private TMP_Text _buttonText = null!;

        [UIComponent("button"), UsedImplicitly] private Button _button = null!;

        #endregion

        #region Setup

        private Action? _buttonAction;
        private Action? _backgroundAction;

        public async void Setup(string previewUrl, string topText, string bottomText, string buttonText, Action buttonAction, Action backgroundAction) {
            _topText.text = $" {topText}";
            _bottomText.text = bottomText;
            _buttonText.text = buttonText;
            _buttonAction = buttonAction;
            _backgroundAction = backgroundAction;
            if (!string.IsNullOrEmpty(previewUrl)) {
                await _image.SetImageAsync(previewUrl);
            }
        }

        public void UpdateBottomText(string bottomText) {
            _bottomText.text = bottomText;
        }

        protected override void OnInitialize() {
            _background._skew = 0.18f;

            _image.material = BundleLoader.RoundTextureMaterial;
            _image._skew = 0.18f;
            _image.__Refresh();

            _topText.overflowMode = TextOverflowModes.Ellipsis;
            _bottomText.overflowMode = TextOverflowModes.Ellipsis;

            SimpleClickHandler.Custom(Content.gameObject, OnBackgroundPressed);
            SmoothHoverController.Custom(Content.gameObject, OnBackgroundHover);
        }

        [UIAction("OnButtonPressed"), UsedImplicitly]
        private void OnButtonPressed() {
            _buttonAction?.Invoke();
        }

        private void OnBackgroundPressed() {
            _backgroundAction?.Invoke();
        }

        private void OnBackgroundHover(bool hovered, float progress) {
            _background.color = new Color(0, 0, 0, 0.5f + 0.4f * progress);
        }

        #endregion
    }
}
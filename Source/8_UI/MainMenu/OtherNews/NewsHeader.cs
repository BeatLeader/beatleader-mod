using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class NewsHeader : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("image"), UsedImplicitly]
        private ImageView _image = null!;

        [UIComponent("text"), UsedImplicitly]
        private TMP_Text _text = null!;
        
        [UIComponent("text"), UsedImplicitly]
        private RectTransform _textRect = null!;

        #endregion

        #region Setup

        public void Setup(string text) {
            _text.text = text;
        }
        
        protected override void OnInitialize() {
            _textRect.anchorMin = Vector2.zero;
            _textRect.anchorMax = Vector2.one;
            _textRect.sizeDelta = Vector2.zero;
            _image._skew = 0.18f;
            _image.__Refresh();
        }

        #endregion
    }
}
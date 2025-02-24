using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostHeaderPanel : ReeUIComponentV2 {
        #region UI Components

        [UIComponent("avatar"), UsedImplicitly]
        private ImageView _avatarImage = null!;

        [UIComponent("name"), UsedImplicitly]
        private TMP_Text _nameText = null!;

        [UIComponent("date"), UsedImplicitly]
        private TMP_Text _dateText = null!;

        [UIComponent("image"), UsedImplicitly]
        private ImageView _image = null!;

        #endregion

        #region Setup

        public async void Setup(string avatarUrl, string name, long timestamp) {
            _nameText.text = name;
            _dateText.text = FormatUtils.GetRelativeTimeString(timestamp, false);
            if (string.IsNullOrEmpty(avatarUrl)) {
                _avatarImage.sprite = BundleLoader.UnknownIcon;
            } else {
                _avatarImage.SetImage(avatarUrl);
            }
        }

        protected override void OnInitialize() {
            _avatarImage.material = BundleLoader.RoundTexture10Material;

            _image._skew = 0.18f;
            _image.__Refresh();
        }

        #endregion
    }
}
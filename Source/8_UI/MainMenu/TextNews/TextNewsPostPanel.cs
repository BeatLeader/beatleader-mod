using BeatLeader.Models;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class TextNewsPostPanel : ReeUIComponentV2 {
        #region UI Components

        [UIValue("header"), UsedImplicitly]
        private TextNewsPostHeaderPanel _header = null!;

        [UIComponent("body"), UsedImplicitly]
        private TMP_Text _bodyText = null!;

        [UIComponent("image"), UsedImplicitly]
        private ImageView _image = null!;

        #endregion

        #region Setup

        public async void Setup(NewsPost post) {
            _bodyText.text = post.body;
            _header.Setup(post.ownerIcon, post.owner, post.timepost);
            var hasImage = !string.IsNullOrEmpty(post.image);
            _image.gameObject.SetActive(hasImage);
            if (hasImage) {
                var options = new BeatSaberUI.ScaleOptions
                {
                    ShouldScale = true,
                    MaintainRatio = true,
                    Width = 512,
                    Height = 400
                };
                await _image.SetImageAsync(post.image, false, options);
            }
        }

        protected override void OnInstantiate() {
            _header = Instantiate<TextNewsPostHeaderPanel>(transform);
        }

        protected override void OnInitialize() {
            _image.material = Instantiate(new Material(Resources.FindObjectsOfTypeAll<Material>().Last(x => x.name == "UINoGlow")));
        }

        #endregion
    }
}
using System;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.MainMenu {
    internal class EarthNewsPanel : ReeUIComponentV2 {
        [UIValue("header"), UsedImplicitly]
        private NewsHeader _header = null!;

        [UIValue("preview-panel"), UsedImplicitly]
        private FeaturedPreviewPanel _previewPanel = null!;

        [UIComponent("background"), UsedImplicitly]
        private ImageView _background = null!;

        private const string EarthUrl = "https://raw.githubusercontent.com/BeatLeader/beatleader-resources/refs/heads/master/Logos/Earth/earth-logo-const.gif";

        protected override void OnInstantiate() {
            _header = Instantiate<NewsHeader>(transform);
            _header.Setup("Recycling Initiative");
            
            _previewPanel = Instantiate<FeaturedPreviewPanel>(transform);
        }

        protected override void OnInitialize() {
            Action openModal = () => ReeModalSystem.OpenModal<EarthEventDialog>(Content.transform, null!);
            
            _previewPanel.Setup(
                EarthUrl,
                "Earth day event",
                "",
                "Join",
                openModal,
                openModal
            );

            var accent = Color.green.ColorWithG(0.5f);
            _previewPanel.SetAccentColor(accent);
        }
    }
}
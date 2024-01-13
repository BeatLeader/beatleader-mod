﻿using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ContextSelector : ReeUIComponentV2 {
        #region Init / Dispose

        [UIComponent("main-button"), UsedImplicitly]
        private ClickableImage _mainButton;

        protected override void OnInitialize() {
            InitializeMainButton();
            PluginConfig.ScoresContextChangedEvent += ApplyContext;
            ApplyContext(PluginConfig.ScoresContext);
        }

        protected override void OnDispose() {
            PluginConfig.ScoresContextChangedEvent -= ApplyContext;
        }

        #endregion

        #region MainButton

        private static readonly Color SelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color HoverColor = new(0.0f, 0.2f, 0.5f, 0.5f);

        private void InitializeMainButton() {
            _mainButton.material = BundleLoader.UIAdditiveGlowMaterial;
            _mainButton.DefaultColor = SelectedColor;
            _mainButton.HighlightColor = HoverColor;
        }

        private void ApplyContext(ScoresContext scoresContext) {
            _mainButton.sprite = scoresContext.Icon();
        }

        [UIAction("main-button-on-click"), UsedImplicitly]
        private void MainButtonOnClick() {
            LeaderboardEvents.NotifyContextSelectorWasPressed();
        }

        #endregion
    }
}
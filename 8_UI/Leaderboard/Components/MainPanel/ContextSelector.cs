using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ContextSelector : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            SetMaterials();
            PluginConfig.ScoresContextChangedEvent += OnScoresContextChanged;
            OnScoresContextChanged(PluginConfig.ScoresContext);
        }

        protected override void OnDispose() {
            PluginConfig.ScoresContextChangedEvent -= OnScoresContextChanged;
        }

        #endregion

        #region OnScoresContextChanged

        private void OnScoresContextChanged(ScoresContext scoresContext) {
            switch (scoresContext) {
                case ScoresContext.Standard:
                    SetColor(_modifiersComponent, false);
                    break;
                case ScoresContext.Modifiers:
                    SetColor(_modifiersComponent, true);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color FadedHoverColor = new(0.5f, 0.5f, 0.5f, 0.2f);

        [UIComponent("modifiers-component"), UsedImplicitly]
        private ClickableImage _modifiersComponent;

        private void SetMaterials() {
            _modifiersComponent.material = BundleLoader.UIAdditiveGlowMaterial;
        }

        private static void SetColor(ClickableImage image, bool selected) {
            image.DefaultColor = selected ? SelectedColor : FadedColor;
            image.HighlightColor = selected ? SelectedColor : FadedHoverColor;
        }

        #endregion

        #region Callbacks

        [UIAction("modifiers-on-click"), UsedImplicitly]
        private void NavModifiersOnClick() {
            PluginConfig.ScoresContext = PluginConfig.ScoresContext switch {
                ScoresContext.Standard => ScoresContext.Modifiers,
                ScoresContext.Modifiers => ScoresContext.Standard,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion
    }
}
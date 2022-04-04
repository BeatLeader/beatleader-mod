using System;
using System.Threading;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ContextSelector.bsml")]
    internal class ContextSelector : ReeUIComponent {
        #region Start

        private void Start() {
            SelectContext(BLContext.DefaultScoresContext);
            SetMaterials();

            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>()) {
                Plugin.Log.Notice(sprite.name);
                Thread.Sleep(1);
            }
        }

        #endregion

        #region SetScope

        private ScoresContext _currentContext;

        private void SelectContext(ScoresContext newContext) {
            switch (newContext) {
                case ScoresContext.Standard:
                    SetColor(_modifiersComponent, false);
                    break;
                case ScoresContext.Modifiers:
                    SetColor(_modifiersComponent, true);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            
            _currentContext = newContext;
            LeaderboardEvents.NotifyContextWasSelected(_currentContext);
        }

        #endregion

        #region Colors

        private static readonly Color SelectedColor = new Color(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color FadedColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        private static readonly Color FadedHoverColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

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
            switch (_currentContext) {
                case ScoresContext.Standard:
                    SelectContext(ScoresContext.Modifiers);
                    break;
                case ScoresContext.Modifiers:
                    SelectContext(ScoresContext.Standard);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
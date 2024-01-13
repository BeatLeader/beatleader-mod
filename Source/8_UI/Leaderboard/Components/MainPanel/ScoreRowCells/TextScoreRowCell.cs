using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class TextScoreRowCell : AbstractScoreRowCell {
        #region Setup

        private Func<object, string> _formatter;

        public void Setup(
            Func<object, string> formatter,
            TextAlignmentOptions alignmentOption = TextAlignmentOptions.Center,
            TextOverflowModes overflowMode = TextOverflowModes.Overflow,
            float fontSize = 3.4f,
            bool richText = true
        ) {
            _formatter = formatter;
            textComponent.alignment = alignmentOption;
            textComponent.overflowMode = overflowMode;
            textComponent.fontSize = fontSize;
            textComponent.richText = richText;
        }

        #endregion

        #region Implementation

        public override void SetValue(object? value) {
            textComponent.text = value == null ? "" : _formatter.Invoke(value);
            isEmpty = false;
        }

        public override void SetAlpha(float value) {
            if (textComponent.overflowMode == TextOverflowModes.Ellipsis) {
                var tmp = (Color)textComponent.faceColor;
                tmp.a = value;
                textComponent.faceColor = tmp;
            } else {
                textComponent.alpha = value;
            }
        }

        protected override float CalculatePreferredWidth() {
            return textComponent.preferredWidth;
        }

        #endregion

        #region TextComponent

        [UIComponent("text-component"), UsedImplicitly]
        public TextMeshProUGUI textComponent;

        #endregion
    }
}
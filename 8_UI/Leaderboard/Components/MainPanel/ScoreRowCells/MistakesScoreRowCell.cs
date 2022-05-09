using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader.Components {
    internal class MistakesScoreRowCell : AbstractScoreRowCell {
        #region Implementation

        private const string GoodColor = "#88FF88";
        private const string BadColor = "#FF8888";

        public void SetValues(int missedNotes, int badCuts, int bombCuts, int wallsHit) {
            var totalMistakes = missedNotes + badCuts + bombCuts + wallsHit;
            textComponent.text = totalMistakes == 0 ? $"<color={GoodColor}>FC" : $"<color={BadColor}>{totalMistakes}<size=70%>x";
            isEmpty = false;
        }

        public override void SetAlpha(float value) {
            textComponent.alpha = value;
        }

        protected override float CalculatePreferredWidth() {
            return textComponent.preferredWidth;
        }

        #endregion

        #region Components

        [UIComponent("text-component"), UsedImplicitly]
        public TextMeshProUGUI textComponent;

        #endregion
    }
}
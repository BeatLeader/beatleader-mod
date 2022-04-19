using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.ScoreInfoPanelControls.bsml")]
    internal class ScoreInfoPanelControls : ReeUIComponent {
        #region SetScore

        private Score _score;

        public void SetScore(Score score) {
            _score = score;
        }

        #endregion
    }
}
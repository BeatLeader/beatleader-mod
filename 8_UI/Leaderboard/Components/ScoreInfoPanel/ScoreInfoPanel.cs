using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.ScoreInfoPanel.bsml")]
    internal class ScoreInfoPanel : ReeUIComponent {
        #region Components

        [UIValue("mini-profile"), UsedImplicitly]
        private MiniProfile _miniProfile = Instantiate<MiniProfile>();

        [UIValue("score-overview"), UsedImplicitly]
        private ScoreOverview _scoreOverview = Instantiate<ScoreOverview>();

        [UIValue("controls"), UsedImplicitly]
        private ScoreInfoPanelControls _controls = Instantiate<ScoreInfoPanelControls>();

        #endregion

        #region SetScore

        public void SetScore(Score score) {
            _miniProfile.SetPlayer(score.player);
            _scoreOverview.SetScore(score);
            _controls.SetScore(score);
        }

        #endregion
    }
}
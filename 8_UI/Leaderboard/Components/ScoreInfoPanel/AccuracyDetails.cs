using System.Text;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModestTree;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ScoreInfoPanel.AccuracyDetails.bsml")]
    internal class AccuracyDetails : ReeUIComponent {
        #region Components

        //TODO: LeftHandDetails
        //TODO: RightHandDetails
        //TODO: AccuracyGrid

        #endregion
        
        #region SetScore

        public void SetScore(Score score) {
            //TODO: SetScore
        }

        #endregion
        
        #region SetActive

        public void SetActive(bool value) {
            Active = value;
        }

        #endregion
        
        #region Active

        private bool _active = true;

        [UIValue("active"), UsedImplicitly]
        private bool Active {
            get => _active;
            set {
                if (_active.Equals(value)) return;
                _active = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}
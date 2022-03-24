using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController
    {
        [UsedImplicitly]
        private string _placeholderText = "WOW, Such panel!";

        [UIValue("placeholder-text")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                if (_placeholderText.Equals(value)) return;
                _placeholderText = value;
                NotifyPropertyChanged();
            }
        }
    }
}
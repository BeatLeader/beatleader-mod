using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController
    {
        [UsedImplicitly]
        private string _placeholderText = "Such scores!";

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
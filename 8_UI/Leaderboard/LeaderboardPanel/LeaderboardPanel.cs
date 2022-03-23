using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;

namespace BeatLeader {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.LeaderboardPanel.bsml")]
    internal class LeaderboardPanel : BSMLAutomaticViewController {
        [UIValue("placeholder-text")] [UsedImplicitly]
        private string _placeholderText = "WOW, Such panel!";
    }
}
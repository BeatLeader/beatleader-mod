using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.PlayerInfo.bsml")]
    internal class PlayerInfo : ReeUIComponent {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.UserProfileStartedEvent += OnProfileRequestStarted;
            LeaderboardEvents.UserProfileFetchedEvent += OnProfileFetched;
        }

        protected override void OnDispose() {
            LeaderboardEvents.UserProfileStartedEvent -= OnProfileRequestStarted;
            LeaderboardEvents.UserProfileFetchedEvent -= OnProfileFetched;
        }

        #endregion

        #region Events

        private void OnProfileRequestStarted() {
            PlaceholderText = "Loading...";
        }

        private void OnProfileFetched(Profile p) {
            if (p == null) {
                PlaceholderText = "Internet error or profile does not exist.";
            } else {
                var txt = $"#{p.rank} ({p.country} #{p.countryRank}) {p.name}\n{p.pp:.00}pp";
                PlaceholderText = txt;
            }
        }

        #endregion

        #region PlaceholderText

        private string _placeholderText = "WOW, Such panel!";

        [UIValue("placeholder-text"), UsedImplicitly]
        public string PlaceholderText {
            get => _placeholderText;
            set {
                if (_placeholderText.Equals(value)) return;
                _placeholderText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}
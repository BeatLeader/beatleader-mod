using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.PlayerInfo.bsml")]
    internal class PlayerInfo : ReeUiComponent {
        #region Initialize/Dispose

        private LeaderboardEvents _leaderboardEvents;

        public void Initialize(LeaderboardEvents leaderboardEvents) {
            _leaderboardEvents = leaderboardEvents;
            _leaderboardEvents.UserProfileStartedEvent += OnProfileRequestStarted;
            _leaderboardEvents.UserProfileFetchedEvent += OnProfileFetched;
        }

        public void Dispose() {
            _leaderboardEvents.UserProfileStartedEvent -= OnProfileRequestStarted;
            _leaderboardEvents.UserProfileFetchedEvent -= OnProfileFetched;
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

        [UsedImplicitly]
        private string _placeholderText = "WOW, Such panel!";

        [UIValue("placeholder-text")]
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
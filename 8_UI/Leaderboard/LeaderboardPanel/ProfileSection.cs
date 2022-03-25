using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader {
    internal partial class LeaderboardPanel {
        #region Init/Dispose

        private void InitializeProfileSection() {
            _leaderboardEvents.UserProfileStartedEvent += OnProfileRequestStarted;
            _leaderboardEvents.UserProfileFetchedEvent += OnProfileFetched;
        }

        private void DisposeProfileSection() {
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
                string txt = String.Format("#{0} ({1} #{2}) {3}\n{4:.00}pp", p.rank, p.country, p.countryRank, p.name, p.pp);
                PlaceholderText = txt;
            }
        }

        #endregion

        #region PlaceholderText

        [UsedImplicitly] private string _placeholderText = "WOW, Such panel!";

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
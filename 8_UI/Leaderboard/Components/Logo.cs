using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.Logo.bsml")]
    internal class Logo : ReeUIComponent {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            LeaderboardEvents.UserProfileStartedEvent += OnProfileRequestStarted;
            LeaderboardEvents.UserProfileFetchedEvent += OnProfileFetched;
            LeaderboardEvents.ScoresRequestStartedEvent += OnScoresRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent += OnScoresRequestFinished;
            SetMaterial();
        }

        protected override void OnDispose() {
            LeaderboardEvents.UserProfileStartedEvent -= OnProfileRequestStarted;
            LeaderboardEvents.UserProfileFetchedEvent -= OnProfileFetched;
            LeaderboardEvents.ScoresRequestStartedEvent -= OnScoresRequestStarted;
            LeaderboardEvents.ScoresFetchedEvent -= OnScoresRequestFinished;
        }

        #endregion

        #region Events

        private void OnScoresRequestStarted() {
            _loadingScores = false;
            UpdateState();
        }

        private void OnScoresRequestFinished(Paged<Score> paged) {
            _loadingScores = true;
            UpdateState();
        }

        private void OnProfileRequestStarted() {
            _loadingProfile = true;
            UpdateState();
        }

        private void OnProfileFetched(Profile p) {
            _loadingProfile = false;
            UpdateState();
        }

        #endregion

        #region UpdateState

        private bool _loadingProfile;
        private bool _loadingScores;

        private void UpdateState() {
            var loading = _loadingProfile || _loadingScores;
            PlaceholderText = loading ? "Icon" : "Spinner";
        }

        #endregion

        #region Image & Material

        [UIComponent("logo-image"), UsedImplicitly]
        private Image _logoImage;

        private Material _materialInstance;

        private void SetMaterial() {
            _materialInstance = Object.Instantiate(BundleLoader.LogoMaterial);
            _logoImage.material = _materialInstance;
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
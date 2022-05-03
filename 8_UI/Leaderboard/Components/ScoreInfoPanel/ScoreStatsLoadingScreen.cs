using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ScoreStatsLoadingScreen : ReeUIComponentV2 {
        #region Initialize

        protected override void OnInitialize() {
            LeaderboardState.ScoreStatsRequest.StateChangedEvent += OnScoreStatsRequestStateChanged;
            OnScoreStatsRequestStateChanged(LeaderboardState.ScoreStatsRequest.State);
        }

        protected override void OnDispose() {
            LeaderboardState.ScoreStatsRequest.StateChangedEvent -= OnScoreStatsRequestStateChanged;
        }

        #endregion

        #region Events

        private const string FailMessage = "<color=#FF8888>Oops!\n<size=60%>Something went wrong";

        private void OnScoreStatsRequestStateChanged(RequestState state) {
            switch (state) {
                case RequestState.Started:
                    Text = "Loading...";
                    break;
                case RequestState.Failed:
                    Text = FailMessage;
                    break;
                case RequestState.Uninitialized:
                case RequestState.Finished:
                    Text = "";
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
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

        #region text

        private string _text = "";

        [UIValue("text"), UsedImplicitly]
        private string Text {
            get => _text;
            set {
                if (_text.Equals(value)) return;
                _text = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}
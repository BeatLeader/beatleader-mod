using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ScoreStatsLoadingScreen : ReeUIComponentV2 {
        #region Initialize

        protected override void OnInitialize() {
            ScoreStatsRequest.AddStateListener(OnScoreStatsRequestStateChanged);
        }

        protected override void OnDispose() {
            ScoreStatsRequest.RemoveStateListener(OnScoreStatsRequestStateChanged);
        }

        #endregion

        #region Events

        private const string FailMessage = "<color=#FF8888>Oops!\n<size=60%>Something went wrong";
        private const string LoadingMessage = "Loading...";

        private void OnScoreStatsRequestStateChanged(API.RequestState state, ScoreStats result, string failReason) {
            Text = state switch {
                API.RequestState.Started => LoadingMessage,
                API.RequestState.Failed => FailMessage,
                _ => ""
            };
        }

        #endregion

        #region SetActive

        public void SetActive(bool value) {
            Active = value;
        }

        #endregion

        #region SetFailed

        public void SetFailed(bool failed) {
            Text = failed ? FailMessage : LoadingMessage;
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
using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class UploadProgressBar : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            InitializeComponents();

            ScoreUtil.ReplayUploadStartedEvent += OnReplayUploadStarted;
            UploadReplayRequest.AddStateListener(OnUploadRequestStateChanged);
            UploadReplayRequest.AddProgressListener(OnUploadRequestProgressChanged);
        }

        protected override void OnDispose() {
            ScoreUtil.ReplayUploadStartedEvent -= OnReplayUploadStarted;
            UploadReplayRequest.RemoveStateListener(OnUploadRequestStateChanged);
            UploadReplayRequest.RemoveProgressListener(OnUploadRequestProgressChanged);
        }

        #endregion

        #region Events

        private void OnReplayUploadStarted(Replay replay) {
            _lastReplay = replay;
            FailsCount = 0;
        }

        private void OnUploadRequestProgressChanged(float uploadProgress, float downloadProgress, float overallProgress) {
            _textComponent.text = $"{(uploadProgress * 100):F2}<size=60%>%";
        }

        private void OnUploadRequestStateChanged(API.RequestState state, Score result, string failReason) {
            _textRoot.gameObject.SetActive(state is API.RequestState.Started);

            switch (state) {
                case API.RequestState.Finished:
                    _lastReplay = null;
                    FailsCount = 0;
                    break;
                case API.RequestState.Failed:
                    FailsCount += 1;
                    break;
            }
        }

        #endregion

        #region Retry logic

        private int _failsCount;
        private Replay _lastReplay;

        private int FailsCount {
            get => _failsCount;
            set {
                _failsCount = value;
                _retryButton.gameObject.SetActive(_failsCount >= 3);
            }
        }

        private void OnRetryClicked() {
            if (_lastReplay == null) return;
            ScoreUtil.UploadReplay(_lastReplay);
        }

        #endregion

        #region Components

        [UIComponent("text-root"), UsedImplicitly]
        private ImageView _textRoot;

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        [UIComponent("retry-button"), UsedImplicitly]
        private Button _retryButton;

        private void InitializeComponents() {
            _textRoot.raycastTarget = false;
            _retryButton.onClick.AddListener(OnRetryClicked);
            FailsCount = 0;
        }

        #endregion
    }
}
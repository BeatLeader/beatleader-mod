using BeatLeader.API.Methods;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader.Components {
    internal class UploadProgressBar : ReeUIComponentV2 {
        #region Init / Dispose

        protected override void OnInitialize() {
            InitializeBackground();

            UploadReplayRequest.AddStateListener(OnUploadRequestStateChanged);
            UploadReplayRequest.AddProgressListener(OnUploadRequestProgressChanged);
        }

        protected override void OnDispose() {
            UploadReplayRequest.RemoveStateListener(OnUploadRequestStateChanged);
            UploadReplayRequest.RemoveProgressListener(OnUploadRequestProgressChanged);
        }

        #endregion

        #region Events

        private void OnUploadRequestProgressChanged(float uploadProgress, float downloadProgress, float overallProgress) {
            _textComponent.text = $"{(uploadProgress * 100):F2}<size=60%>%";
        }

        private void OnUploadRequestStateChanged(API.RequestState state, Score result, string failReason) {
            _root.gameObject.SetActive(state is API.RequestState.Started);
        }

        #endregion

        #region Components

        [UIComponent("root"), UsedImplicitly]
        private ImageView _root;

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        private void InitializeBackground() {
            _root.raycastTarget = false;
        }

        #endregion
    }
}
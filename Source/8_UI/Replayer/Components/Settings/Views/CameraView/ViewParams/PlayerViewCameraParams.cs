using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class PlayerViewCameraParams : ReeUIComponentV3<PlayerViewCameraParams>, SettingsCameraView.ICameraViewParams {
        #region ViewParams

        public bool SupportsCameraView(ICameraView cameraView) {
            return cameraView is PlayerViewCameraView;
        }

        public void Setup(Transform? trans, ICameraView? cameraView) {
            Content.SetActive(trans is not null);
            ContentTransform.SetParent(trans, false);
            _cameraView = cameraView as PlayerViewCameraView;
            RefreshView();
        }

        #endregion

        #region View

        private PlayerViewCameraView? _cameraView;
        private float _smoothness;
        private Vector3 _positionOffset;
        private Quaternion _rotationOffset;

        private void RefreshView() {
            if (_cameraView is null) return;
            _cameraView.smoothness = _smoothness;
            _cameraView.positionOffset = _positionOffset;
            _cameraView.rotationOffset = _rotationOffset;
        }

        #endregion

        #region Callbacks

        [UIAction("smoothness-change"), UsedImplicitly]
        private void HandleSmoothnessChange(float value) {
            _smoothness = value;
            RefreshView();
        }

        [UIAction("position-offset-change"), UsedImplicitly]
        private void HandlePositionOffsetChanged(Vector3 vector) {
            _positionOffset = vector;
            RefreshView();
        }

        [UIAction("rotation-offset-change"), UsedImplicitly]
        private void HandleRotationOffsetChanged(Vector3 vector) {
            _rotationOffset = Quaternion.Euler(vector);
            RefreshView();
        }

        #endregion
    }
}
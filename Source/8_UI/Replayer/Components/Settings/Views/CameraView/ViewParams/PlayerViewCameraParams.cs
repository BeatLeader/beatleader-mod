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
            LoadFromView();
        }

        #endregion

        #region View

        private PlayerViewCameraView? _cameraView;
        private float _smoothness;
        private Vector3 _positionOffset;
        private Quaternion _rotationOffset;

        private void LoadFromView() {
            if (_cameraView is null) return;
            _smoothness = _cameraView.Smoothness;
            _positionOffset = _cameraView.PositionOffset;
            _rotationOffset = _cameraView.RotationOffset;
        }
        
        private void RefreshView() {
            if (_cameraView is null) return;
            _cameraView.Smoothness = _smoothness;
            _cameraView.PositionOffset = _positionOffset;
            _cameraView.RotationOffset = _rotationOffset;
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
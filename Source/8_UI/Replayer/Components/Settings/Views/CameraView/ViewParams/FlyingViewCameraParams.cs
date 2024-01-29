using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    internal class FlyingViewCameraParams : ReeUIComponentV3<FlyingViewCameraParams>, SettingsCameraView.ICameraViewParams {
        #region ViewParams

        public bool SupportsCameraView(ICameraView cameraView) {
            return cameraView is FlyingCameraView;
        }

        public void Setup(Transform? trans, ICameraView? cameraView) {
            Content.SetActive(trans is not null);
            ContentTransform.SetParent(trans, false);
            _cameraView = cameraView as FlyingCameraView;
            RefreshView();
        }

        #endregion

        #region View

        private FlyingCameraView? _cameraView;
        private float _speed;
        private Vector2 _sensitivity;

        private void RefreshView() {
            if (_cameraView is null) return;
            _cameraView.flySpeed = _speed;
            _cameraView.mouseSensitivity = _sensitivity;
        }

        #endregion

        #region Callbacks

        [UIAction("speed-change"), UsedImplicitly]
        private void HandleSpeedChanged(float speed) {
            _speed = speed;
            RefreshView();
        }

        [UIAction("sensitivity-change"), UsedImplicitly]
        private void HandleSensitivityChanged(Vector3 vector) {
            _sensitivity = vector;
            RefreshView();
        }

        [UIAction("position-reset"), UsedImplicitly]
        private void HandlePositionReset() {
            _cameraView?.Reset();
        }

        #endregion
    }
}
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Movement;
using BeatLeader.Utils;
using IPA.Utilities;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer
{
    public class SettingsLoader : IInitializable, IDisposable
    {
        [Inject] private readonly PlayerDataModel _playerDataModel;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly ReplayLaunchData _replayData;
        [InjectOptional] private readonly UI2DManager _uiManager;

        public virtual void Initialize()
        {
            if (_playerDataModel != null && _cameraController != null)
            {
                if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris)
                    _cameraController.CullingMask |= 1 << LayerMasks.noteDebrisLayer;
                else
                    _cameraController.CullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
            }

            if (_uiManager != null)
            {
                _uiManager.UIVisibilityChangedEvent += HandleUIVisibilityChanged;
                _uiManager.ShowUI = _replayData.ActualSettings.ShowUI;
            }

            if (_cameraController != null)
            {
                if (InputManager.IsInFPFC)
                {
                    _cameraController.FieldOfView = _replayData.ActualSettings.CameraFOV;
                    _cameraController.CameraFOVChangedEvent += HandleCameraFOVChanged;
                }
                _cameraController.SetCameraPose(InputManager.IsInFPFC ?
                    _replayData.ActualSettings.FPFCCameraPose : _replayData.ActualSettings.VRCameraPose);
                _cameraController.CameraPoseChangedEvent += HandleCameraPoseChanged;
            }
        }
        public virtual void Dispose()
        {
            if (_cameraController != null)
            {
                _cameraController.CameraFOVChangedEvent -= HandleCameraFOVChanged;
                _cameraController.CameraPoseChangedEvent -= HandleCameraPoseChanged;
            }

            if (_uiManager != null) _uiManager.UIVisibilityChangedEvent -= HandleUIVisibilityChanged;
        }

        private void HandleUIVisibilityChanged(bool visible)
        {
            _replayData.ActualToWriteSettings.ShowUI = visible;
        }
        private void HandleCameraFOVChanged(int fov)
        {
            _replayData.ActualSettings.CameraFOV = fov;
        }
        private void HandleCameraPoseChanged(ICameraPoseProvider poseProvider)
        {
            var settings = _replayData.ActualToWriteSettings;
            settings.FPFCCameraPose = InputManager.IsInFPFC ? poseProvider.Name : settings.FPFCCameraPose;
            settings.VRCameraPose = !InputManager.IsInFPFC ? poseProvider.Name : settings.VRCameraPose;
        }
    }
}

using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using BeatLeader.Replayer.Emulation;
using BeatLeader.Utils;
using System;
using Zenject;

namespace BeatLeader.Replayer
{
    public class SettingsLoader : IInitializable, IDisposable
    {
        [Inject] private readonly PlayerDataModel _playerDataModel;
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly ReplayLaunchData _launchData;
        [Inject] private readonly VRControllersProvider _controllersProvider;
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
                _uiManager.ShowUI = _launchData.ActualSettings.ShowUI;
            }

            if (_cameraController != null)
            {
                if (InputManager.IsInFPFC)
                {
                    _cameraController.FieldOfView = _launchData.ActualSettings.CameraFOV;
                    _cameraController.CameraFOVChangedEvent += HandleCameraFOVChanged;
                }
                _cameraController.SetCameraPose(InputManager.IsInFPFC ?
                    _launchData.ActualSettings.FPFCCameraPose : _launchData.ActualSettings.VRCameraPose);
                _cameraController.CameraPoseChangedEvent += HandleCameraPoseChanged;
            }

            if (_controllersProvider != null)
            {
                var settings = _launchData.ActualSettings;

                _controllersProvider.Head.gameObject.SetActive(settings.ShowHead);
                _controllersProvider.LeftSaber.gameObject.SetActive(settings.ShowLeftSaber);
                _controllersProvider.RightSaber.gameObject.SetActive(settings.ShowRightSaber);
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
            _launchData.ActualToWriteSettings.ShowUI = visible;
        }
        private void HandleCameraFOVChanged(int fov)
        {
            _launchData.ActualSettings.CameraFOV = fov;
        }
        private void HandleCameraPoseChanged(ICameraPoseProvider poseProvider)
        {
            var settings = _launchData.ActualToWriteSettings;
            settings.FPFCCameraPose = InputManager.IsInFPFC ? poseProvider.Name : settings.FPFCCameraPose;
            settings.VRCameraPose = !InputManager.IsInFPFC ? poseProvider.Name : settings.VRCameraPose;
        }
    }
}

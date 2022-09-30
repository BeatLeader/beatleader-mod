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
        [InjectOptional] private readonly PlayerDataModel _playerDataModel;
        [InjectOptional] private readonly ReplayerCameraController _cameraController;
        [InjectOptional] private readonly VRControllersProvider _controllersProvider;
        [InjectOptional] private readonly ReplayLaunchData _replayData;
        [InjectOptional] private readonly UI2DManager _2DManager;

        private RoomAdjustSettingsViewController _roomAdjustViewController;
        private OffsetsApplier _cameraOffsetsApplier;
        private OffsetsApplier _handsOffsetsApplier;
        private Vector3SO _roomPosition;
        private FloatSO _roomRotation;

        private UnityEngine.Vector3 _tempPosition;
        private float _tempRotation;

        public virtual void Initialize()
        {
            bool roomWasApplied = false;

            _roomAdjustViewController = Resources.FindObjectsOfTypeAll<RoomAdjustSettingsViewController>().FirstOrDefault();
            if (_roomAdjustViewController != null)
            {
                _roomPosition = _roomAdjustViewController.GetField<Vector3SO, RoomAdjustSettingsViewController>("_roomCenter");
                _roomRotation = _roomAdjustViewController.GetField<FloatSO, RoomAdjustSettingsViewController>("_roomRotation");

                _tempPosition = _roomPosition;
                _tempRotation = _roomRotation;

                _roomAdjustViewController.ResetRoom();
                roomWasApplied = true;
            }

            if (_playerDataModel != null && _cameraController != null)
            {
                if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris)
                    _cameraController.CullingMask |= 1 << LayerMasks.noteDebrisLayer;
                else
                    _cameraController.CullingMask &= ~(1 << LayerMasks.noteDebrisLayer);

                if (roomWasApplied && !InputManager.IsInFPFC)
                {
                    _cameraOffsetsApplier = new GameObject("CameraOffsetsApplier").AddComponent<OffsetsApplier>();
                    _handsOffsetsApplier = new GameObject("HandsOffsetsApplier").AddComponent<OffsetsApplier>();

                    _cameraOffsetsApplier.Setup(_cameraController.transform);

                    var pose = new Pose(_tempPosition, UnityEngine.Quaternion.Euler(0, _roomRotation, 0));
                    _cameraOffsetsApplier.Offsets = pose;

                    if (_controllersProvider != null)
                    {
                        _handsOffsetsApplier.Setup(_controllersProvider.MenuHandsContainerTransform);
                        _handsOffsetsApplier.Offsets = pose;
                    }
                }
            }

            if (_2DManager != null)
            {
                _2DManager.UIVisibilityChangedEvent += HandleUIVisibilityChanged;
                _2DManager.ShowUI = _replayData.actualSettings.ShowUI;
            }

            if (_cameraController != null)
            {
                if (InputManager.IsInFPFC)
                {
                    _cameraController.FieldOfView = _replayData.actualSettings.CameraFOV;
                    _cameraController.CameraFOVChangedEvent += HandleCameraFOVChanged;
                }
                _cameraController.SetCameraPose(InputManager.IsInFPFC ?
                    _replayData.actualSettings.FPFCCameraPose : _replayData.actualSettings.VRCameraPose);
                _cameraController.CameraPoseChangedEvent += HandleCameraPoseChanged;
            }
        }
        public virtual void Dispose()
        {
            if (_roomAdjustViewController != null)
            {
                _roomPosition.value = _tempPosition;
                _roomRotation.value = _tempRotation;
            }

            if (_cameraController != null)
            {
                _cameraController.CameraFOVChangedEvent -= HandleCameraFOVChanged;
                _cameraController.CameraPoseChangedEvent -= HandleCameraPoseChanged;
            }

            if (_2DManager != null) _2DManager.UIVisibilityChangedEvent -= HandleUIVisibilityChanged;
        }

        private void HandleUIVisibilityChanged(bool visible)
        {
            _replayData.actualToWriteSettings.ShowUI = visible;
        }
        private void HandleCameraFOVChanged(int fov)
        {
            _replayData.actualSettings.CameraFOV = fov;
        }
        private void HandleCameraPoseChanged(ICameraPoseProvider poseProvider)
        {
            var settings = _replayData.actualToWriteSettings;
            settings.FPFCCameraPose = InputManager.IsInFPFC ? poseProvider.Name : settings.FPFCCameraPose;
            settings.VRCameraPose = !InputManager.IsInFPFC ? poseProvider.Name : settings.VRCameraPose;
        }
    }
}

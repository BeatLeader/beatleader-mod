using BeatLeader.Interop;
using BeatLeader.Replayer.Camera;
using IPA.Config.Data;
using IPA.Utilities;
using SiraUtil.Tools.FPFC;
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
        [InjectOptional] private readonly Models.ReplayLaunchData _replayData;
        [InjectOptional] private readonly UI2DManager _2DManager;

        private RoomAdjustSettingsViewController _roomAdjustViewController;
        private Vector3SO _roomPosition;
        private FloatSO _roomRotation;

        private Vector3 _tempPosition;
        private float _tempRotation;

        public virtual void Initialize()
        {
            if (_playerDataModel != null && _cameraController != null)
            {
                if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris)
                    _cameraController.CullingMask |= 1 << LayerMasks.noteDebrisLayer;
                else
                    _cameraController.CullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
            }

            _roomAdjustViewController = Resources.FindObjectsOfTypeAll<RoomAdjustSettingsViewController>().FirstOrDefault();
            if (_roomAdjustViewController != null)
            {
                _roomPosition = _roomAdjustViewController.GetField<Vector3SO, RoomAdjustSettingsViewController>("_roomCenter");
                _roomRotation = _roomAdjustViewController.GetField<FloatSO, RoomAdjustSettingsViewController>("_roomRotation");

                _tempPosition = _roomPosition;
                _tempRotation = _roomRotation;

                _roomAdjustViewController.ResetRoom();
            }

            if (_2DManager != null)
            {
                _2DManager.OnUIVisibilityChanged += x => _replayData.actualToWriteSettings.ShowUI = x;
                _2DManager.showUI = _replayData.actualSettings.ShowUI;
            }

            RaycastBlocker.EnableBlocker = true;
        }
        public virtual void Dispose()
        {
            if (_roomAdjustViewController != null)
            {
                _roomPosition.value = _tempPosition;
                _roomRotation.value = _tempRotation;
            }

            RaycastBlocker.EnableBlocker = false;
        }
    }
}

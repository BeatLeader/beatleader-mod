using BeatLeader.Interop;
using BeatLeader.Replayer.Camera;
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

        private RoomAdjustSettingsViewController _roomAdjustViewController;
        private Vector3SO _roomPosition;
        private FloatSO _roomRotation;

        private Vector3 _tempPosition;
        private float _tempRotation;

        public virtual void Initialize()
        {
            if (_playerDataModel != null && _cameraController != null)
            {
                bool showDebris = !_playerDataModel.playerData.playerSpecificSettings.reduceDebris;
                showDebris = _replayData != null && _replayData.overrideSettings ? _replayData.settings.showDebris : showDebris;

                if (showDebris)
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

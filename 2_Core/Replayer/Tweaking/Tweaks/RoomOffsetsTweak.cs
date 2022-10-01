using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using System.Linq;
using BeatLeader.Replayer.Movement;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class RoomOffsetsTweak : GameTweak
    {
        public override bool CanBeInstalled => !InputManager.IsInFPFC;

        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly VRControllersProvider _controllersProvider;

        private RoomAdjustSettingsViewController _roomAdjustViewController;
        private OffsetsApplier _cameraOffsetsApplier;
        private OffsetsApplier _handsOffsetsApplier;

        private Vector3SO _roomPosition;
        private FloatSO _roomRotation;

        private Vector3 _tempRoomPosition;
        private float _tempRoomRotation;

        public override void Initialize()
        {
            _roomAdjustViewController =
                Resources.FindObjectsOfTypeAll<RoomAdjustSettingsViewController>().FirstOrDefault();
            if (_roomAdjustViewController == null) return;

            _tempRoomPosition = _roomPosition =
                _roomAdjustViewController.GetField<Vector3SO, RoomAdjustSettingsViewController>("_roomCenter");
            _tempRoomRotation = _roomRotation =
                _roomAdjustViewController.GetField<FloatSO, RoomAdjustSettingsViewController>("_roomRotation");

            _roomAdjustViewController.ResetRoom();
            CreateOffsetsAppliers();
        }
        public override void Dispose()
        {
            _roomPosition.value = _tempRoomPosition;
            _roomRotation.value = _tempRoomRotation;

            if (_cameraOffsetsApplier != null)
            {
                _cameraOffsetsApplier.Dispose();
                _cameraOffsetsApplier.TryDestroy();
            }
            if (_handsOffsetsApplier != null)
            {
                _handsOffsetsApplier.Dispose();
                _handsOffsetsApplier.TryDestroy();
            }
        }

        private void CreateOffsetsAppliers()
        {
            _cameraOffsetsApplier = new GameObject("CameraOffsetsApplier").AddComponent<OffsetsApplier>();
            _handsOffsetsApplier = new GameObject("HandsOffsetsApplier").AddComponent<OffsetsApplier>();

            _cameraOffsetsApplier.Setup(_cameraController.transform);
            _handsOffsetsApplier.Setup(_controllersProvider.MenuHandsContainerTransform);

            var pose = new Pose(_tempRoomPosition, Quaternion.Euler(0, _roomRotation, 0));
            _cameraOffsetsApplier.Offsets = pose;
            _handsOffsetsApplier.Offsets = pose;
        }
    }
}
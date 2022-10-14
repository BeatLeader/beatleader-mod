using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using BeatLeader.Replayer.Emulation;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class RoomOffsetsTweak : GameTweak
    {
        [Inject] private readonly ReplayerCameraController _cameraController;
        [Inject] private readonly VRControllersProvider _controllersProvider;
        [FirstResource] private readonly MainSettingsModelSO _mainSettingsModel;

        private OffsetsApplier _cameraOffsetsApplier;
        private OffsetsApplier _handsOffsetsApplier;

        private Vector3SO _roomPosition;
        private FloatSO _roomRotation;

        private Vector3 _tempRoomPosition;
        private float _tempRoomRotation;

        public override void Initialize()
        {            
            this.LoadResources();
            
            _tempRoomPosition = _roomPosition = _mainSettingsModel.roomCenter;
            _tempRoomRotation = _roomRotation = _mainSettingsModel.roomRotation;

            _roomPosition.value = new Vector3(0f, 0f, 0f);
            _roomRotation.value = 0f;
            if (!InputUtils.IsInFPFC) CreateOffsetsAppliers();
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
            _handsOffsetsApplier.Setup(_controllersProvider.MenuHandsContainer);

            var pose = new Pose(_tempRoomPosition, Quaternion.Euler(0, _tempRoomRotation, 0));
            _cameraOffsetsApplier.Offsets = pose;
            _handsOffsetsApplier.Offsets = pose;
        }
    }
}

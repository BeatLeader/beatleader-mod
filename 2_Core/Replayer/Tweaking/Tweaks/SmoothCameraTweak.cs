using BeatLeader.Replayer.Camera;
using BeatLeader.Utils;
using IPA.Utilities;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class SmoothCameraTweak : GameTweak
    {
        public override bool CanBeInstalled => !InputManager.IsInFPFC;

        [Inject] private readonly ReplayerCameraController _cameraController;

        private SmoothCamera _smoothCamera;
        private MainCamera _originalCamera;
        private MainCamera _fakeCamera = new();

        public override void LateInitialize()
        {
            _smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(x => x.transform.parent.name == "LocalPlayerGameCore");

            _fakeCamera.SetField("_camera", _cameraController.Camera);
            _fakeCamera.SetField("_transform", _cameraController.Camera.transform);

            _originalCamera = _smoothCamera.GetField<MainCamera, SmoothCamera>("_mainCamera");
            _smoothCamera.SetField("_mainCamera", _fakeCamera);
            _smoothCamera.gameObject.SetActive(true);
        }
        public override void Dispose()
        {
            if (_originalCamera != null)
                _smoothCamera?.SetField("_mainCamera", _originalCamera);
        }
    }
}

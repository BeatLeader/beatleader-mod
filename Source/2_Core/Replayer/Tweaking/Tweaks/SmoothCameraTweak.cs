using BeatLeader.Utils;
using IPA.Utilities;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Tweaking {
    internal class SmoothCameraTweak : GameTweak {
        public override bool CanBeInstalled => !InputUtils.IsInFPFC;

        [Inject] private readonly ReplayerCameraController _cameraController = null!;

        private readonly MainCamera _fakeCamera = new();
        private SmoothCamera _smoothCamera = null!;
        private MainCamera _originalCamera = null!;

        public override void LateInitialize() {
            _smoothCamera = Resources.FindObjectsOfTypeAll<SmoothCamera>()
                .FirstOrDefault(x => x.transform
                .parent.name == "LocalPlayerGameCore"
                && x.gameObject.activeInHierarchy);

            _fakeCamera.SetField("_camera", _cameraController.ViewableCamera.Camera!);
            _fakeCamera.SetField("_transform", _cameraController.ViewableCamera.CameraContainer);

            _originalCamera = _smoothCamera.GetField<MainCamera, SmoothCamera>("_mainCamera");
            _smoothCamera.SetField("_mainCamera", _fakeCamera);
            _smoothCamera.gameObject.SetActive(true);
        }
        public override void Dispose() {
            if (_originalCamera != null)
                _smoothCamera?.SetField("_mainCamera", _originalCamera);
        }
    }
}

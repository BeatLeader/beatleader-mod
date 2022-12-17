using UnityEngine.XR;
using UnityEngine;
using Zenject;
using IPA.Utilities;

namespace BeatLeader.Replayer.Emulation {
    internal class OriginalVRControllersProvider : MonoBehaviour, Models.IVRControllersProvider {
        [Inject] private readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] private readonly PlayerTransforms _playerTransforms;

        public VRController LeftSaber { get; private set; }
        public VRController RightSaber { get; private set; }
        public VRController Head { get; private set; }

        public void ShowControllers(bool show = true) {
            LeftSaber.gameObject.SetActive(show);
            RightSaber.gameObject.SetActive(show);
            Head.gameObject.SetActive(show);
        }

        private void Awake() {
            Head = new GameObject("ReplayerFakeHead").AddComponent<VRControllerEmulator>();
            Head.node = XRNode.Head;

            var monke = Instantiate(BundleLoader.MonkeyPrefab, null, false);
            monke.transform.localEulerAngles = new(0, 180, 0);
            monke.transform.SetParent(Head.transform, false);

            LeftSaber = _vrControllersManager.leftHandVRController;
            RightSaber = _vrControllersManager.rightHandVRController;
            _playerTransforms.SetField("_headTransform", Head.transform);
            _vrControllersManager.DisableAllVRControllers();

            ShowControllers(false);
        }
    }
}

using UnityEngine.XR;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class OriginalVRControllersProvider : MonoBehaviour, Models.IVRControllersProvider {
        [Inject] private readonly PlayerVRControllersManager _vrControllersManager = null!;

        public VRController LeftSaber { get; private set; } = null!;
        public VRController RightSaber { get; private set; } = null!;
        public VRController Head { get; private set; } = null!;

        public void ShowControllers(bool show = true) {
            LeftSaber.gameObject.SetActive(show);
            RightSaber.gameObject.SetActive(show);
            Head.gameObject.SetActive(show);
        }

        private void Awake() {
            Head = new GameObject("ReplayerFakeHead").AddComponent<VRController>();
            Head.node = XRNode.Head;
            Head.enabled = false;

            var monke = Instantiate(BundleLoader.MonkeyPrefab, null, false);
            monke.transform.localEulerAngles = new(0, 180, 0);
            monke.transform.SetParent(Head.transform, false);

            LeftSaber = _vrControllersManager.leftHandVRController;
            RightSaber = _vrControllersManager.rightHandVRController;
            _vrControllersManager.DisableAllVRControllers();

            Head.transform.SetParent(LeftSaber.transform.parent, false);

            //ShowControllers(false);
        }
    }
}

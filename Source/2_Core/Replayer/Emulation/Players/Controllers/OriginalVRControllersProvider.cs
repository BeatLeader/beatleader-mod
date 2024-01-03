using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class OriginalVRControllersProvider : MonoBehaviour, Models.IHandVRControllersProvider {
        [Inject] private readonly PlayerVRControllersManager _vrControllersManager = null!;

        public VRController LeftHand { get; private set; } = null!;
        public VRController RightHand { get; private set; } = null!;

        public void ShowControllers(bool show = true) {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
        }

        private void Awake() {
            LeftHand = _vrControllersManager.leftHandVRController;
            RightHand = _vrControllersManager.rightHandVRController;
            _vrControllersManager.DisableAllVRControllers();
        }
    }
}

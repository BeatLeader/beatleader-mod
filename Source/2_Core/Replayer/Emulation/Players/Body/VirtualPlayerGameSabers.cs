using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerGameSabers : IVirtualPlayerBody {
        [Inject]
        private readonly PlayerVRControllersManager _vrControllersManager = null!;
        
        private Transform _leftController = null!;
        private Transform _rightController = null!;

        [Inject, UsedImplicitly]
        private void Init() {
            _leftController = _vrControllersManager.leftHandVRController.transform;
            _rightController = _vrControllersManager.rightHandVRController.transform;

            _vrControllersManager.GetComponent<DeactivateVRControllersOnFocusCapture>().enabled = false;
            _vrControllersManager.DisableAllVRControllers();
        }

        public void ApplySettings(BasicBodySettings basicBodySettings) {
            _leftController.gameObject.SetActive(basicBodySettings.LeftSaberEnabled);
            _rightController.gameObject.SetActive(basicBodySettings.RightSaberEnabled);
        }

        public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
            _leftController.SetLocalPose(leftHandPose);
            _rightController.SetLocalPose(rightHandPose);
        }
    }
}
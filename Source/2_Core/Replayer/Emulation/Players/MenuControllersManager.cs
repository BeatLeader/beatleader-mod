using BeatLeader.Utils;
using BeatSaber.GameSettings;
using IPA.Utilities;
using UnityEngine;
using VRUIControls;
using Zenject;

namespace BeatLeader.Replayer {
    public class MenuControllersManager : MonoBehaviour {
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjects = null!;
        [Inject] private readonly PauseMenuManager _pauseMenuManager = null!;
        [Inject] private readonly DiContainer _diContainer = null!; 
        [Inject] private readonly VRInputModule _vrInputModule = null!;
        [Inject] private readonly SettingsManager _settingsManager = null!;
        [Inject] private readonly ControllerProfilesModel _controllersProfile = null!;

        public Transform HandsContainer { get; private set; } = null!;
        public VRController LeftHand { get; private set; } = null!;
        public VRController RightHand { get; private set; } = null!;

        public void ShowHands(bool show = true) {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
            HandsContainer.gameObject.SetActive(show);
        }

        private void Awake() {
            this.LoadResources();

            var menuHandsTransform = _pauseMenuManager.transform.Find("MenuControllers");
            menuHandsTransform.gameObject.GetComponent<DeactivateAnimatorOnInputFocusCapture>().TryDestroy();
            LeftHand = Instantiate(menuHandsTransform.Find("ControllerLeft")).GetComponent<VRController>();
            RightHand = Instantiate(menuHandsTransform.Find("ControllerRight")).GetComponent<VRController>();

            ((VRControllersValueSettingsOffsets)LeftHand._transformOffset).SetField("_settingsManager", _settingsManager);
            ((VRControllersValueSettingsOffsets)RightHand._transformOffset).SetField("_settingsManager", _settingsManager);

            ((VRControllersValueSettingsOffsets)LeftHand._transformOffset).SetField("_controllersProfile", _controllersProfile);
            ((VRControllersValueSettingsOffsets)RightHand._transformOffset).SetField("_controllersProfile", _controllersProfile);

            _diContainer.InjectComponentsInChildren(LeftHand.gameObject);
            _diContainer.InjectComponentsInChildren(RightHand.gameObject);

            HandsContainer = new GameObject("ReplayerHands").transform;
            HandsContainer.SetParent(_extraObjects.ReplayerCenterAdjust, false);

            LeftHand.transform.SetParent(HandsContainer, false);
            RightHand.transform.SetParent(HandsContainer, false);
            SetInputControllers(LeftHand, RightHand);
            ShowHands(!InputUtils.UsesFPFC);
        }

        private void SetInputControllers(VRController left, VRController right) {
            var pointer = _vrInputModule.GetField<VRPointer, VRInputModule>("_vrPointer");
            pointer.SetField("_leftVRController", left);
            pointer.SetField("_rightVRController", right);
        }
    }
}

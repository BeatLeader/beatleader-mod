using IPA.Utilities;
using UnityEngine;
using UnityEngine.XR;
using Zenject;
using BeatLeader.Utils;
using VRUIControls;
using BeatLeader.Replayer.Emulation;

namespace BeatLeader.Replayer {
    public class ReplayerControllersManager : MonoBehaviour {
        [Inject] private readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly PlayerTransforms _playerTransforms;
        [Inject] private readonly DiContainer _diContainer;
        [Inject] private readonly VRInputModule _vrInputModule;

        [FirstResource] private readonly MainSettingsModelSO _mainSettingsModel;

        public VRController LeftSaber { get; protected set; }
        public VRController RightSaber { get; protected set; }
        public VRController Head { get; protected set; }

        public VRController LeftHand { get; protected set; }
        public VRController RightHand { get; protected set; }
        public Transform HandsContainer { get; protected set; }
        [FirstResource("VRGameCore")] public Transform Origin { get; protected set; }

        public void ShowHands(bool show = true) {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
            HandsContainer.gameObject.SetActive(show);
        }

        private void Awake() {
            this.LoadResources();
            SetupHands();

            Head = new GameObject("ReplayerFakeHead").AddComponent<VRControllerEmulator>();
            Head.transform.SetParent(Origin, false);
            Head.node = XRNode.Head;

            var monke = Instantiate(BundleLoader.MonkeyPrefab, null, false);
            monke.transform.localEulerAngles = new Vector3(0, 180, 0);
            monke.transform.SetParent(Head.transform, false);

            LeftSaber = _vrControllersManager.leftHandVRController;
            RightSaber = _vrControllersManager.rightHandVRController;
            _playerTransforms.SetField("_headTransform", Head.transform);
            _vrControllersManager.DisableAllVRControllers();

            SetInputControllers(LeftHand, RightHand);
            InjectControllers();
            ShowHands(false);
        }

        private void SetupHands() {
            var menuHandsTransform = _pauseMenuManager.transform.Find("MenuControllers");
            LeftHand = Instantiate(menuHandsTransform.Find("ControllerLeft")).GetComponent<VRController>();
            RightHand = Instantiate(menuHandsTransform.Find("ControllerRight")).GetComponent<VRController>();

            _diContainer.InjectComponentsInChildren(LeftHand.gameObject);
            _diContainer.InjectComponentsInChildren(RightHand.gameObject);

            HandsContainer = new GameObject("ReplayerHands").transform;
            HandsContainer.SetParent(Origin, true);
            HandsContainer.transform.localPosition = _mainSettingsModel.roomCenter;
            HandsContainer.transform.localEulerAngles = new Vector3(0, _mainSettingsModel.roomRotation, 0);

            LeftHand.transform.SetParent(HandsContainer, false);
            RightHand.transform.SetParent(HandsContainer, false);
        }

        private void SetInputControllers(VRController left, VRController right) {
            var pointer = _vrInputModule.GetField<VRPointer, VRInputModule>("_vrPointer");
            pointer.SetField("_leftVRController", left);
            pointer.SetField("_rightVRController", right);
        }

        private void InjectControllers() {
            foreach (var item in GetType().GetProperties()) {
                if (!item.CanWrite || item.PropertyType != typeof(VRController)) continue;

                var value = item.GetValue(this);
                if (value == null) continue;

                _diContainer.Inject(item.GetValue(this));
            }
        }
    }
}

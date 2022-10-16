using IPA.Utilities;
using UnityEngine;
using UnityEngine.XR;
using Zenject;
using BeatLeader.Utils;

namespace BeatLeader.Replayer.Emulation {
    public class VRControllersProvider : MonoBehaviour {
        [Inject] private readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly PlayerTransforms _playerTransforms;
        [Inject] private readonly DiContainer _diContainer;
        [FirstResource] private readonly MainSettingsModelSO _mainSettingsModel;

        public VRController LeftSaber { get; protected set; }
        public VRController RightSaber { get; protected set; }
        public VRController Head { get; protected set; }

        public VRController LeftHand { get; protected set; }
        public VRController RightHand { get; protected set; }
        public Transform MenuHandsContainer { get; protected set; }
        [FirstResource("VRGameCore")] public Transform Origin { get; protected set; }

        private Transform _menuHandsTransform;
        private bool _isInitialized;

        public void ShowMenuControllers(bool show = true) {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
            _menuHandsTransform.gameObject.SetActive(show);
        }

        private void Awake() {
            if (_isInitialized) return;
            this.LoadResources();
            _menuHandsTransform = _pauseMenuManager.transform.Find("MenuControllers");
            LeftHand = _menuHandsTransform.Find("ControllerLeft")?.GetComponent<VRController>();
            RightHand = _menuHandsTransform.Find("ControllerRight")?.GetComponent<VRController>();

            Head = new GameObject("ReplayerFakeHead").AddComponent<VRControllerEmulator>();
            Head.transform.SetParent(Origin, false);
            Head.node = XRNode.Head;

            var monke = Instantiate(BundleLoader.MonkeyPrefab, null, false);
            monke.transform.localEulerAngles = new Vector3(0, 180, 0);
            monke.transform.SetParent(Head.transform, false);

            MenuHandsContainer = new GameObject("PauseMenuHands").transform;
            MenuHandsContainer.SetParent(Origin, true);
            _menuHandsTransform.SetParent(MenuHandsContainer, false);
            MenuHandsContainer.transform.localPosition = _mainSettingsModel.roomCenter;
            MenuHandsContainer.transform.localEulerAngles = new Vector3(0, _mainSettingsModel.roomRotation, 0);

            _vrControllersManager.leftHandVRController.enabled = false;
            _vrControllersManager.rightHandVRController.enabled = false;

            _playerTransforms.SetField("_headTransform", Head.transform);
            _vrControllersManager.SetField("_leftHandVRController",
                LeftSaber = _vrControllersManager.leftHandVRController);
            _vrControllersManager.SetField("_rightHandVRController",
                RightSaber = _vrControllersManager.rightHandVRController);

            InjectControllers();
            _isInitialized = true;
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

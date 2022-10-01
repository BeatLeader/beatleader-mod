using IPA.Utilities;
using UnityEngine;
using UnityEngine.XR;
using Zenject;
using BeatLeader.Utils;

namespace BeatLeader.Replayer.Movement
{
    public class VRControllersProvider : MonoBehaviour
    {
        [Inject] private readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] private readonly PauseMenuManager _pauseMenuManager;
        [Inject] private readonly PlayerTransforms _playerTransforms;
        [Inject] private readonly DiContainer _diContainer;

        public VRController LeftSaber { get; protected set; }
        public VRController RightSaber { get; protected set; }
        public VRController Head { get; protected set; }

        public VRController LeftHand { get; protected set; }
        public VRController RightHand { get; protected set; }
        public Transform MenuHandsContainer { get; protected set; }
        [FirstResource("VRGameCore")] public Transform OriginTransform { get; protected set; }

        private Transform _menuHandsTransform;
        private bool _isInitialized;

        public void ShowMenuControllers(bool show = true)
        {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
            _menuHandsTransform.gameObject.SetActive(show);
        }

        private void Awake()
        {
            if (_isInitialized) return;

            this.LoadResources();

            _menuHandsTransform = _pauseMenuManager.transform.Find("MenuControllers");
            LeftHand = _menuHandsTransform.Find("ControllerLeft")?.GetComponent<VRController>();
            RightHand = _menuHandsTransform.Find("ControllerRight")?.GetComponent<VRController>();

            Head = Instantiate(BundleLoader.MonkeyPrefab, null, false).AddComponent<VRControllerEmulator>();
            Head.transform.SetParent(OriginTransform, false);
            Head.node = XRNode.Head;

            //you ask me why? smth just moves menu hands to the zero pose
            MenuHandsContainer = new GameObject("PauseMenuHands").transform;
            MenuHandsContainer.SetParent(OriginTransform, true);
            _menuHandsTransform.SetParent(MenuHandsContainer, false);

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
        private void InjectControllers()
        {
            foreach (var item in GetType().GetProperties())
            {
                if (!item.CanWrite || item.PropertyType != typeof(VRController)) continue;

                var value = item.GetValue(this);
                if (value == null) continue;

                _diContainer.Inject(item.GetValue(this));
            }
        }
    }
}

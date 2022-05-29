using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.XR;
using Zenject;

namespace BeatLeader.Replays.Movement
{
    public class VRControllersManager : MonoBehaviour
    {
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] protected readonly PlayerTransforms _playerTransforms;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;

        protected VRController _leftSaber;
        protected VRController _rightSaber;
        protected VRController _leftHand;
        protected VRController _rightHand;
        protected VRController _head;
        protected GameObject _handsContainer;
        protected bool _initialized;

        public VRController leftSaber => _leftSaber;
        public VRController rightSaber => _rightSaber;
        public VRController leftHand => _leftHand;
        public VRController rightHand => _rightHand;
        public VRController head => _head;
        public GameObject handsContainer => _handsContainer;
        public bool initialized => _initialized;

        public void Start()
        {
            _handsContainer = _pauseMenuManager.transform.Find("MenuControllers").gameObject;
            _leftHand = _handsContainer.transform.Find("ControllerLeft").GetComponent<VRController>();
            _rightHand = _handsContainer.transform.Find("ControllerRight").GetComponent<VRController>();

            //creating fake head
            GameObject head = new GameObject("ReplayerFakeHead");
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            head.AddComponent<BoxCollider>().size = new Vector3(1, 3, 1);
            _head = head.AddComponent<VRControllerWrapper>();
            _head.node = XRNode.Head;

            _playerTransforms.SetField("_headTransform", _head.transform);

            //sabers patch
            GameObject leftGO = _vrControllersManager.leftHandVRController.gameObject;
            GameObject rightGO = _vrControllersManager.rightHandVRController.gameObject;

            leftGO.SetActive(false);
            rightGO.SetActive(false);
            Destroy(_vrControllersManager.leftHandVRController);
            Destroy(_vrControllersManager.rightHandVRController);

            _leftSaber = leftGO.AddComponent<VRControllerWrapper>();
            _rightSaber = rightGO.AddComponent<VRControllerWrapper>();
            leftSaber.node = XRNode.LeftHand;
            rightSaber.node = XRNode.RightHand;

            leftGO.gameObject.SetActive(true);
            rightGO.gameObject.SetActive(true);

            _vrControllersManager.SetField("_leftHandVRController", _leftSaber);
            _vrControllersManager.SetField("_rightHandVRController", _rightSaber);
            _initialized = true;
        }
        public void ShowMenuControllers()
        {
            _leftHand.gameObject.SetActive(true);
            _rightHand.gameObject.SetActive(true);
        }
        public void HideMenuControllers()
        {
            _leftHand.gameObject.SetActive(false);
            _rightHand.gameObject.SetActive(false);
        }
    }
}

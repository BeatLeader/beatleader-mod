using System;
using System.Reflection;
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
        [Inject] protected readonly DiContainer _diContainer;

        protected List<(Transform, XRNode)> _attachedObjects = new List<(Transform, XRNode)>();

        protected VRController _leftSaber;
        protected VRController _rightSaber;
        protected VRController _leftHand;
        protected VRController _rightHand;
        protected VRController _head;
        protected GameObject _handsContainer;
        protected bool _initialized;

        public VRController leftSaber => _leftSaber;
        public VRController rightSaber => _rightSaber;
        public VRController head => _head;
        public VRController leftHand => _leftHand;
        public VRController rightHand => _rightHand;
        public GameObject handsContainer => _handsContainer;
        public bool initialized => _initialized;

        public void Awake()
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
            InjectControllers();
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
        public void AttachToTheHand(XRNode hand, Transform transform)
        {
            if (_attachedObjects.Contains((transform, hand))) return;
            switch (hand)
            {
                case XRNode.LeftHand:
                    transform.SetParent(_leftHand.transform);
                    break;
                case XRNode.RightHand:
                    transform.SetParent(_rightHand.transform);
                    break;
                default:
                    Debug.LogWarning("You can attach object only to one of the hands!");
                    return;
            }
            _attachedObjects.Add((transform, hand));
        }
        public void DetachFromTheHand(Transform transform)
        {
            (Transform, XRNode) node = default;
            foreach (var item in _attachedObjects)
            {
                if (item.Item1 == transform)
                {
                    node = item;
                }
            }
            if (node.Item1 != null)
                _attachedObjects.Remove(node);
            else
                Debug.LogWarning("This object is not attached to the hand!");
        }
        protected void InjectControllers()
        {
            foreach (var item in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (item.FieldType == typeof(VRController))
                {
                    _diContainer.Inject(item.GetValue(this));
                }
            }
        }
    }
}

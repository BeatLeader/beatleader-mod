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

        protected Dictionary<Transform, (XRNode, Transform)> _attachedObjects = new();

        public VRController leftSaber { get; protected set; }
        public VRController rightSaber { get; protected set; }
        public VRController head { get; protected set; }
        public VRController leftHand { get; protected set; }
        public VRController rightHand { get; protected set; }
        public GameObject handsContainer { get; protected set; }
        public bool initialized { get; protected set; }

        private void Awake()
        {
            handsContainer = _pauseMenuManager.transform.Find("MenuControllers").gameObject;
            leftHand = handsContainer.transform.Find("ControllerLeft").GetComponent<VRController>();
            rightHand = handsContainer.transform.Find("ControllerRight").GetComponent<VRController>();

            //creating fake head
            GameObject fakeHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fakeHead.name = "ReplayerFakeHead";
            fakeHead.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            fakeHead.AddComponent<BoxCollider>().size = new Vector3(1, 3, 1);
            head = fakeHead.AddComponent<VRControllerWrapper>();
            head.node = XRNode.Head;

            _playerTransforms.SetField("_headTransform", head.transform);

            //sabers patch
            GameObject leftGO = _vrControllersManager.leftHandVRController.gameObject;
            GameObject rightGO = _vrControllersManager.rightHandVRController.gameObject;

            leftGO.SetActive(false);
            rightGO.SetActive(false);
            Destroy(_vrControllersManager.leftHandVRController);
            Destroy(_vrControllersManager.rightHandVRController);

            leftSaber = leftGO.AddComponent<VRControllerWrapper>();
            rightSaber = rightGO.AddComponent<VRControllerWrapper>();
            leftSaber.node = XRNode.LeftHand;
            rightSaber.node = XRNode.RightHand;

            leftGO.gameObject.SetActive(true);
            rightGO.gameObject.SetActive(true);

            _vrControllersManager.SetField("_leftHandVRController", leftSaber);
            _vrControllersManager.SetField("_rightHandVRController", rightSaber);
            InjectControllers();
            initialized = true;
        }
        public void ShowMenuControllers(bool show = true)
        {
            leftHand.gameObject.SetActive(show);
            rightHand.gameObject.SetActive(show);
        }
        public void ShowNode(XRNode node, bool show = true)
        {
            GameObject go = node switch
            {
                XRNode.Head => head.gameObject,
                XRNode.LeftHand => leftSaber.gameObject,
                XRNode.RightHand => rightSaber.gameObject
            };
            go?.SetActive(show);
        }
        public void AttachToTheNode(XRNode node, Transform transform)
        {
            if (_attachedObjects.ContainsKey(transform)) return;
            Transform originalParent = transform.parent;
            switch (node)
            {
                case XRNode.Head:
                    transform.SetParent(head.transform, false);
                    break;
                case XRNode.LeftHand:
                    transform.SetParent(leftHand.transform, false);
                    break;
                case XRNode.RightHand:
                    transform.SetParent(rightHand.transform, false);
                    break;
                default:
                    return;
            }
            _attachedObjects.Add(transform, (node, originalParent));
        }
        public void DetachFromTheNode(Transform transform, bool reparentToOriginalParent)
        {
            (XRNode, Transform) pair = _attachedObjects.First(x => x.Key == transform).Value;
            if (pair != (null, null))
            {
                transform.SetParent(reparentToOriginalParent ? pair.Item2 : null);
                _attachedObjects.Remove(transform);
            }
            else Debug.LogWarning("This object is not attached to the any from nodes!");
        }
        protected void InjectControllers()
        {
            foreach (var item in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (item.CanWrite && item.PropertyType == typeof(VRController))
                {
                    _diContainer.Inject(item.GetValue(this));
                }
            }
        }
    }
}

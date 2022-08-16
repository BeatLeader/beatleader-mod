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

namespace BeatLeader.Replayer.Movement
{
    public class VRControllersManager : MonoBehaviour
    {
        [Inject] protected readonly ReplayerManualInstaller.InitData _initData;
        [Inject] protected readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] protected readonly PlayerTransforms _playerTransforms;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;
        [Inject] protected readonly DiContainer _diContainer;

        protected Dictionary<Transform, (XRNode, Transform)> _attachedObjects = new();

        public VRController LeftSaber { get; protected set; }
        public VRController RightSaber { get; protected set; }
        public VRController Head { get; protected set; }
        public VRController LeftHand { get; protected set; }
        public VRController RightHand { get; protected set; }
        public GameObject HandsContainer { get; protected set; }
        public bool IsInitialized { get; protected set; }

        protected virtual void Awake()
        {
            HandsContainer = _pauseMenuManager.transform.Find("MenuControllers").gameObject;
            LeftHand = HandsContainer.transform.Find("ControllerLeft").GetComponent<VRController>();
            RightHand = HandsContainer.transform.Find("ControllerRight").GetComponent<VRController>();

            //creating fake head
            GameObject fakeHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fakeHead.name = "ReplayerFakeHead";
            fakeHead.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            fakeHead.AddComponent<BoxCollider>().size = new Vector3(1, 3, 1);
            Head = fakeHead.AddComponent<VRControllerWrapper>();
            Head.node = XRNode.Head;

            _playerTransforms.SetField("_headTransform", Head.transform);

            //sabers patch
            GameObject leftGO = _vrControllersManager.leftHandVRController.gameObject;
            GameObject rightGO = _vrControllersManager.rightHandVRController.gameObject;

            leftGO.SetActive(false);
            rightGO.SetActive(false);
            Destroy(_vrControllersManager.leftHandVRController);
            Destroy(_vrControllersManager.rightHandVRController);

            LeftSaber = leftGO.AddComponent<VRControllerWrapper>();
            RightSaber = rightGO.AddComponent<VRControllerWrapper>();
            LeftSaber.node = XRNode.LeftHand;
            RightSaber.node = XRNode.RightHand;

            leftGO.gameObject.SetActive(true);
            rightGO.gameObject.SetActive(true);

            _vrControllersManager.SetField("_leftHandVRController", LeftSaber);
            _vrControllersManager.SetField("_rightHandVRController", RightSaber);
            InjectControllers();
            IsInitialized = true;
        }
        public void ShowMenuControllers(bool show = true)
        {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
            HandsContainer.gameObject.SetActive(show);
        }
        public void ShowNode(XRNode node, bool show = true)
        {
            GameObject go = node switch
            {
                XRNode.Head => Head.gameObject,
                XRNode.LeftHand => LeftSaber.gameObject,
                XRNode.RightHand => RightSaber.gameObject,
                XRNode.GameController => HandsContainer.gameObject
            };
            go?.SetActive(show);
        }
        public void AttachToTheNode(XRNode node, Transform transform)
        {
            if (_attachedObjects.ContainsKey(transform)) return;
            Transform originalParent = transform.parent;
            transform.SetParent(node switch
            {
                XRNode.Head => Head.transform,
                XRNode.LeftHand => LeftHand.transform,
                XRNode.RightHand => RightHand.transform,
                XRNode.GameController => HandsContainer.transform,
                _ => originalParent
            }, false);
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
                if (item.CanWrite && item.PropertyType == typeof(VRController))
                    _diContainer.Inject(item.GetValue(this));
        }
    }
}

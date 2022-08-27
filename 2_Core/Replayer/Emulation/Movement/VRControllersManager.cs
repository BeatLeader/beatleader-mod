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
        [Inject] protected readonly PlayerVRControllersManager _vrControllersManager;
        [Inject] protected readonly PlayerTransforms _playerTransforms;
        [Inject] protected readonly PauseMenuManager _pauseMenuManager;
        [Inject] protected readonly LocksController _softLocksController;
        [Inject] protected readonly MainCamera _mainCamera;
        [Inject] protected readonly DiContainer _diContainer;

        protected Dictionary<Transform, (XRNode, Transform)> _attachedObjects = new();

        public VRController LeftSaber { get; protected set; }
        public VRController RightSaber { get; protected set; }
        public VRController Head { get; protected set; }
        public VRController LeftHand { get; protected set; }
        public VRController RightHand { get; protected set; }
        public Transform MenuHandsContainerTranform { get; protected set; }
        public Transform MenuHandsTranform { get; protected set; }
        public Transform OriginTransform { get; protected set; }
        public bool IsInitialized { get; protected set; }

        protected virtual void Awake()
        {
            MenuHandsTranform = _pauseMenuManager.transform.Find("MenuControllers");
            LeftHand = MenuHandsTranform.Find("ControllerLeft").GetComponent<VRController>();
            RightHand = MenuHandsTranform.Find("ControllerRight").GetComponent<VRController>();
            OriginTransform = Resources.FindObjectsOfTypeAll<Transform>().First(x => x.gameObject.name == "VRGameCore");

            _mainCamera.GetComponentInChildren<BoxCollider>().gameObject.SetActive(false);
            Head = Instantiate(BundleLoader.MonkeyPrefab, null, false).AddComponent<VRControllerWrapper>();
            Head.transform.GetChild(0).eulerAngles = new Vector3(0, 180, 0);
            Head.node = XRNode.Head;

            //you ask me why? smth just moving menu hands to the zero pose
            MenuHandsContainerTranform = new GameObject("PauseMenuHands").transform;
            MenuHandsTranform.SetParent(MenuHandsContainerTranform, false);
            MenuHandsContainerTranform.SetParent(OriginTransform, true);
            Head.transform.SetParent(OriginTransform, false);

            _vrControllersManager.leftHandVRController.enabled = false;
            _vrControllersManager.rightHandVRController.enabled = false;

            LeftSaber = _vrControllersManager.leftHandVRController;
            RightSaber = _vrControllersManager.rightHandVRController;

            _playerTransforms.SetField("_headTransform", Head.transform);
            _vrControllersManager.SetField("_leftHandVRController", LeftSaber);
            _vrControllersManager.SetField("_rightHandVRController", RightSaber);

            InjectControllers();
            IsInitialized = true;
        }
        public void ShowMenuControllers(bool show = true)
        {
            LeftHand.gameObject.SetActive(show);
            RightHand.gameObject.SetActive(show);
            MenuHandsTranform.gameObject.SetActive(show);
        }
        public void ShowNode(XRNode node, bool show = true)
        {
            var go = node switch
            {
                XRNode.Head => Head.gameObject,
                XRNode.LeftHand => LeftSaber.gameObject,
                XRNode.RightHand => RightSaber.gameObject,
                XRNode.GameController => MenuHandsContainerTranform.gameObject,
                _ => null
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
                XRNode.GameController => MenuHandsContainerTranform,
                _ => originalParent
            }, false);
            _attachedObjects.Add(transform, (node, originalParent));
        }
        public void DetachFromTheNode(Transform transform, bool reparentToOriginalParent)
        {
            if (_attachedObjects.TryGetValue(transform, out var pair))
            {
                Plugin.Log.Warn("[Binder] This object is not attached to any node!");
                return;
            }

            transform.SetParent(reparentToOriginalParent ? pair.Item2 : null);
            _attachedObjects.Remove(transform);
        }
        protected void InjectControllers()
        {
            foreach (var item in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                if (item.CanWrite && item.PropertyType == typeof(VRController))
                    _diContainer.Inject(item.GetValue(this));
        }
    }
}

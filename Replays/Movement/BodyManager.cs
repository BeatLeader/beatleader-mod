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
    public class BodyManager : MonoBehaviour
    {
        [Inject] protected readonly ReplayManualInstaller.InitData _initData;
        [Inject] protected readonly PlayerTransforms _playerTransforms;

        protected Dictionary<GameObject, XRNode> _bindedObjects;
        protected FakeVRController _leftHand;
        protected FakeVRController _rightHand;
        protected FakeVRController _head;
        protected bool _initialized;

        public FakeVRController leftHand => _leftHand;
        public FakeVRController rightHand => _rightHand;
        public FakeVRController head => _head;
        public bool initialized => _initialized;

        public void Start()
        {
            _bindedObjects = new Dictionary<GameObject, XRNode>();

            //creating fake head
            GameObject head = new GameObject("ReplayerFakeHead");
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            head.AddComponent<BoxCollider>().size = new Vector3(1, 3, 1);
            _head = head.AddComponent<FakeVRController>();
            _head.node = XRNode.Head;

            _playerTransforms.SetField("_headTransform", _head.transform);

            //sabers patch
            VRController left = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.name == "LeftHand");
            VRController right = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.name == "RightHand");
            GameObject leftGO = left.gameObject;
            GameObject rightGO = right.gameObject;

            leftGO.SetActive(false);
            rightGO.SetActive(false);
            Destroy(left);
            Destroy(right);

            _leftHand = leftGO.AddComponent<FakeVRController>();
            _rightHand = rightGO.AddComponent<FakeVRController>();
            leftHand.node = XRNode.LeftHand;
            rightHand.node = XRNode.RightHand;

            leftGO.gameObject.SetActive(true);
            rightGO.gameObject.SetActive(true);

            if (_initData.noteSyncMode)
            {
                Resources.FindObjectsOfTypeAll<CuttingManager>().FirstOrDefault().enabled = false;
                Destroy(_leftHand.GetComponentInChildren<BoxCollider>());
                Destroy(_rightHand.GetComponentInChildren<BoxCollider>());
            }
            _initialized = true;
        }
        public void BindObjectTo(XRNode node, GameObject @object)
        {
            if (!_initialized) return;
            switch (node)
            {
                case XRNode.LeftHand:
                    if (GetObjectBinding(@object) != XRNode.LeftHand)
                        @object.transform.SetParent(_leftHand.transform);
                    else
                    {
                        Plugin.Log.Warn($"{@object.name} is already binded to the LeftHand node!");
                        return;
                    }
                    break;
                case XRNode.RightHand:
                    if (GetObjectBinding(@object) != XRNode.RightHand)
                        @object.transform.SetParent(_rightHand.transform);
                    else
                    {
                        Plugin.Log.Warn($"{@object.name} is already binded to the RightHand node!");
                        return;
                    }
                    break;
                case XRNode.Head:
                    if (GetObjectBinding(@object) != XRNode.Head)
                        @object.transform.SetParent(_head.transform);
                    else
                    {
                        Plugin.Log.Warn($"{@object.name} is already binded to the Head node!");
                        return;
                    }
                    break;
                default:
                    Plugin.Log.Warn("You can bind only to LeftHand, RightHand and Head nodes!");
                    return;
            }
            if (_bindedObjects.ContainsKey(@object))
                _bindedObjects.Remove(@object);
            _bindedObjects.Add(@object, node);
        }
        public void UnbindObject(GameObject @object)
        {
            if (_bindedObjects.ContainsKey(@object))
            {
                @object.transform.SetParent(null);
                _bindedObjects.Remove(@object);
            }
            else Plugin.Log.Warn("Can't unbind {@object} because it's not binded to any node!");
        }
        public void UnbindObjectTo(GameObject @object, Transform parent)
        {
            if (_bindedObjects.ContainsKey(@object))
            {
                @object.transform.SetParent(parent);
                _bindedObjects.Remove(@object);
            }
            else Plugin.Log.Warn("Can't unbind {@object} because it's not binded to any node!");
        }
        protected XRNode GetObjectBinding(GameObject @object)
        {
            if (!_initialized) return XRNode.GameController;
            XRNode node = XRNode.GameController;
            _bindedObjects.TryGetValue(@object, out node);
            return node;
        }
    }
}

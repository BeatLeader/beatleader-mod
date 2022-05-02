using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Movement
{
    public class BodyManager : MonoBehaviour
    {
        [Inject] protected readonly ReplayManualInstaller.InitData _initData;
        [Inject] protected readonly PlayerTransforms _playerTransforms;

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
            _playerTransforms.SetField("_headTransform", CreateFakeHead().transform);
            PatchOriginalSabers();
            if (_initData.noteSyncMode)
            {
                Resources.FindObjectsOfTypeAll<CuttingManager>().FirstOrDefault().enabled = false;
                Destroy(_leftHand.GetComponentInChildren<BoxCollider>());
                Destroy(_rightHand.GetComponentInChildren<BoxCollider>());
            }
            _initialized = true;
        }
        private void PatchOriginalSabers()
        {
            var left = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.name == "LeftHand");
            var right = Resources.FindObjectsOfTypeAll<VRController>().First(x => x.name == "RightHand");

            var leftGO = left.gameObject;
            var rightGO = right.gameObject;

            leftGO.SetActive(false);
            rightGO.SetActive(false);

            Destroy(left);
            Destroy(right);

            _leftHand = leftGO.AddComponent<FakeVRController>();
            _rightHand = rightGO.AddComponent<FakeVRController>();

            leftHand.node = UnityEngine.XR.XRNode.LeftHand;
            rightHand.node = UnityEngine.XR.XRNode.RightHand;

            leftGO.gameObject.SetActive(true);
            rightGO.gameObject.SetActive(true);
        }
        private GameObject CreateFakeHead()
        {
            //GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject head = new GameObject("FakeHead");
            head.name = "ReplayFakeHead";
            head.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            var collider = head.AddComponent<BoxCollider>();
            collider.size = new Vector3(1, 3, 1);
            _head = head.AddComponent<FakeVRController>();
            _head.node = UnityEngine.XR.XRNode.Head;
            return head;
        }
    }
}

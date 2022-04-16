using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class FakePlayerManager : MonoBehaviour
    {
        private GameObject _head;
        private GameObject _leftHand;
        private GameObject _rightHand;

        private GameObject _leftSaber;
        private GameObject _rightSaber;

        public GameObject head => head;

        public void Init()
        {
            _head = Resources.FindObjectsOfTypeAll<BoxCollider>().First(x => x.gameObject.name == "Head").gameObject;
            _leftHand = Resources.FindObjectsOfTypeAll<BoxCollider>().First(x => x.name == "LeftHand").gameObject;
            _rightHand = Resources.FindObjectsOfTypeAll<BoxCollider>().First(x => x.name == "RightHand").gameObject;

            _head.transform.SetParent(GameObject.CreatePrimitive(PrimitiveType.Sphere).transform, false);
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace BeatLeader.Replays.Emulators
{
    public class ReplayVRController : VRController
    {
        public override void Update() { }
        public void SetTransform(Vector3 pos, Quaternion rot)
        {
            _lastTrackedPosition = pos;
            base.transform.localPosition = pos;
            base.transform.localRotation = rot;
        }
        public void SetPosition(Vector3 position) => _lastTrackedPosition = position;
        public void SetRotation(Quaternion quaternion) => base.transform.localRotation = quaternion;
    }
}

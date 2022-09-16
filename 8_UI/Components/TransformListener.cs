using BeatLeader.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components
{
    public class TransformListener : MonoBehaviour
    {
        public event Action<Pose> OnPoseChanged;

        public Transform transformToListen;
        public bool isListening;
        private Pose _pose;

        public void StartListening()
        {
            isListening = true;
        }
        public void StopListening()
        {
            isListening = false;
        }
        private void Update()
        {
            if (!isListening || transformToListen == null) return;

            var currentPose = transformToListen.GetPose();
            if (_pose != currentPose) OnPoseChanged?.Invoke(currentPose);
            _pose = currentPose;
        }
    }
}

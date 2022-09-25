using BeatLeader.Utils;
using System;
using UnityEngine;

namespace BeatLeader.Components
{
    public class TransformListener : MonoBehaviour
    {
        public Transform TransformToListen
        {
            get => _transformToListen;
            set
            {
                _transformToListen = value;
                _canListen = _transformToListen != null;
            }
        }

        public event Action<Pose> PoseChangedEvent;

        public bool isListening;
        private Transform _transformToListen;
        private bool _canListen;
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
            if (!isListening || !_canListen) return;

            var currentPose = TransformToListen.GetPose();
            if (_pose != currentPose) PoseChangedEvent?.Invoke(currentPose);
            _pose = currentPose;
        }
    }
}

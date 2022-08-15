using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class FollowingVRView : MonoBehaviour
    {
        private Transform _headTransform;
        private Transform _viewTransform;
        public float maximumAngleDifference;
        public float lerpSmoothness;

        private float _nextAngle;

        public void Init(Transform head, Transform view, float difference = 60, float smoothness = 5)
        {
            _headTransform = head;
            _viewTransform = view;
            maximumAngleDifference = difference;
            lerpSmoothness = smoothness;
        }
        private void Update()
        {
            var angle = (_headTransform.eulerAngles - _viewTransform.eulerAngles).y;
            _nextAngle = Mathf.Abs(angle) > maximumAngleDifference ? Mathf.RoundToInt(_headTransform.eulerAngles.y) : _nextAngle;

            var rot = _viewTransform.eulerAngles;
            rot.y = Mathf.Lerp(rot.y, _nextAngle, Time.deltaTime * lerpSmoothness);
            _viewTransform.eulerAngles = rot;
        }
    }
}

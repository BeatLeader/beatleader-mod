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
        public float smoothnessFactor;

        private Quaternion _nextRot;
        private bool _allowHandling;

        public void Init(Transform head, Transform view, float difference = 60, float smoothness = 10)
        {
            _headTransform = head;
            _viewTransform = view;
            maximumAngleDifference = difference;
            smoothnessFactor = smoothness;
        }
        private void Update()
        {
            var headRot = _headTransform.eulerAngles;
            var viewRot = _viewTransform.eulerAngles;

            headRot.x = 0;
            headRot.z = 0;
            viewRot.x = 0;
            viewRot.z = 0;

            var headQuat = Quaternion.Euler(headRot);
            var viewQuat = Quaternion.Euler(viewRot);

            if (Quaternion.Angle(headQuat, viewQuat) > maximumAngleDifference)
            {
                _nextRot = Quaternion.Euler(0, _headTransform.eulerAngles.y, 0);
                _allowHandling = true;
            }
            Animate();
        }
        private void Animate()
        {
            if (!_allowHandling) return;
            _viewTransform.rotation = Quaternion.Lerp(_viewTransform.rotation, _nextRot, Mathf.Clamp(Time.deltaTime * smoothnessFactor, 0, 1));
            _allowHandling = _viewTransform.eulerAngles.y == _nextRot.eulerAngles.y ? false : _allowHandling;
        }
    }
}

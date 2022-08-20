using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class RotatingVRView : MonoBehaviour
    {
        public enum Direction
        {
            Left,
            Right
        }

        public int maximumAngleDifference;
        public int centerAngle;
        public float smoothnessFactor;
        public bool followHead = true;

        private Transform _headTransform;
        private Transform _viewTransform;
        private Quaternion _nextRot;
        private bool _animating;

        public void Init(Transform head, Transform view, int difference = 60, float smoothness = 10)
        {
            _headTransform = head;
            _viewTransform = view;
            maximumAngleDifference = difference;
            smoothnessFactor = smoothness;
        }
        private void Update()
        {
            if (followHead)
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
                    _nextRot = Quaternion.Euler(0, GetClosestCoordinate(headRot.y), 0);
                    _animating = true;
                }
            }
            Animate();
        }
        private void Animate()
        {
            if (!_animating) return;

            var slerp = Mathf.Clamp(Time.deltaTime * smoothnessFactor, 0, 1);
            var rot = Quaternion.Lerp(_viewTransform.rotation, _nextRot, slerp);

            _viewTransform.rotation = rot;
            _animating = Math.Abs(_viewTransform.eulerAngles.y - _nextRot.eulerAngles.y) > 0.01f;
        }
        private int GetClosestCoordinate(float rot)
        {
            int length = 360 / maximumAngleDifference;
            int point1 = 0;
            int point2 = 0;

            for (int i = 0; i < length; i++)
            {
                int val = i * maximumAngleDifference;
                if (val > rot)
                {
                    point1 = val - maximumAngleDifference;
                    point2 = val;
                    break;
                }
            }

            return rot > point2 - (maximumAngleDifference / 2) ? point2 : point1;
        }

        public void Rotate(Direction direction)
        {
            if (followHead || _animating) return;

            var viewRot = _viewTransform.eulerAngles;
            var val = direction switch
            {
                Direction.Left => maximumAngleDifference * -1,
                Direction.Right => maximumAngleDifference,
                _ => 0
            };

            viewRot.y = GetClosestCoordinate(viewRot.y + val);
            _nextRot = Quaternion.Euler(viewRot);
            _animating = true;
        }
        public void RotateCenter()
        {
            if (followHead || _animating) return;

            var viewRot = _viewTransform.eulerAngles;
            viewRot.y = GetClosestCoordinate(centerAngle);
            _nextRot = Quaternion.Euler(viewRot);
            _animating = true;
        }
    }
}

using BeatLeader.Utils;
using System;
using UnityEngine;

namespace BeatLeader.Components
{
    internal class SimpleSmoothAnimator : MonoBehaviour
    {
        public int smoothness = 10;
        private Pose _preAnimPose;
        private Pose _pose;
        private bool _animating;

        public event Action OnAnimationStart;
        public event Action OnAnimationEnd;

        public void StartAnimation(Pose pose)
        {
            _pose = pose;
            _preAnimPose = transform.GetPose();
            _animating = true;
            OnAnimationStart?.Invoke();
        }
        public void StopAnimation(bool returnToLastPose = false)
        {
            _animating = false;
            if (returnToLastPose) transform.SetPose(_preAnimPose);
            OnAnimationEnd?.Invoke();
        }
        private void Update()
        {
            if (!_animating) return;

            var slerp = smoothness * Time.deltaTime;
            var pos = Vector3.Lerp(transform.position, _pose.position, slerp);
            var rot = Quaternion.Lerp(transform.rotation, _pose.rotation, slerp);
            transform.SetPositionAndRotation(pos, rot);

            if (transform.position.Compare(_pose.position, 0.01f)
                    && transform.eulerAngles.Compare(_pose.rotation.eulerAngles, 0.01f))
                StopAnimation();
        }
    }
}
